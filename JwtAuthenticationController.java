package ru.glab.docservice.controller;

import io.swagger.annotations.Api;
import io.swagger.annotations.ApiOperation;
import org.bouncycastle.asn1.*;
import org.bouncycastle.asn1.cms.Attribute;
import org.bouncycastle.asn1.cms.AttributeTable;
import org.bouncycastle.cert.X509CertificateHolder;
import org.bouncycastle.cms.CMSException;
import org.bouncycastle.cms.SignerId;
import org.bouncycastle.cms.SignerInformation;
import org.bouncycastle.cms.SignerInformationVerifier;
import org.bouncycastle.cms.jcajce.JcaSimpleSignerInfoVerifierBuilder;
import org.bouncycastle.operator.OperatorCreationException;
import org.bouncycastle.util.CollectionStore;
import org.bouncycastle.util.encoders.Base64;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.security.authentication.DisabledException;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.userdetails.UsernameNotFoundException;
import org.springframework.web.bind.annotation.*;
import ru.CryptoPro.CAdES.CAdESSignature;
import ru.CryptoPro.CAdES.CAdESSigner;
import ru.CryptoPro.CAdES.CAdESType;
import ru.CryptoPro.CAdES.exception.CAdESException;
import ru.CryptoPro.Crypto.CryptoProvider;
import ru.CryptoPro.JCP.JCP;
import ru.CryptoPro.JCSP.JCSP;
import ru.CryptoPro.reprov.RevCheck;
import ru.CryptoPro.reprov.certpath.LDAPCertStore;
import ru.CryptoPro.reprov.x509.X509CertImpl;
import ru.glab.docservice.config.JwtTokenUtil;
import ru.glab.docservice.model.CurrentUser;
import ru.glab.docservice.model.JwtRequest;
import ru.glab.docservice.model.postgres.User;
import ru.glab.docservice.service.postgres.UsersService;
import java.security.Security;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.*;

import static ru.glab.docservice.util.SecurityConstants.AUTHENTICATE_URL;

@RestController
@CrossOrigin(origins = "*")
@Api(tags = "Аутентификация", description = " ")
public class JwtAuthenticationController {

    @Autowired
    private AuthenticationManager authenticationManager;

    @Autowired
    private JwtTokenUtil jwtTokenUtil;

    @Autowired
    private UsersService usersService;

    private static final Logger logger = LoggerFactory.getLogger(JwtAuthenticationController.class);

    private Map<String, Object> msgMap = new HashMap<>();

    private static boolean signTypeValid = false;
    private static boolean resultValid = false;

    @CrossOrigin(origins = "*")
    @PostMapping(value = AUTHENTICATE_URL)
    @ApiOperation(value = "Запрос на создание токена",
            notes = "ru.glab.docservice.controller.JwtAuthenticationController.createAuthenticationToken",
            tags = "Аутентификация")
    public ResponseEntity<?> createAuthenticationToken(@RequestBody String signature) {
        msgMap.clear();
        logger.info("Вызван метод createAuthenticationToken");

        //  Проверка подписи на Null;
        if (signature == null || signature.length() == 0 ) {
            msgMap.put("error","Пустая подпись!");
            logger.warn("Пустая подпись!");
            return new ResponseEntity<>(msgMap, HttpStatus.BAD_REQUEST);
        }

        // Проверка ЭЦП
        try {
            validate(signature);
        } catch (Exception e) {
            msgMap.put("error", "Подпись невалидная");
            logger.warn("Подпись невалидная");
            return new ResponseEntity<>(msgMap, HttpStatus.UNAUTHORIZED);
        }

        try {
            // Исходная подпись в виде потока байтов из файла.
            byte[] decodingArray = Base64.decode(signature);

            // Декодирует совмещенную подпись с автоопределение типо.
            CAdESSignature cAdESSignatureLong = new CAdESSignature(decodingArray, null, null);
            CAdESSigner signer = cAdESSignatureLong.getCAdESSignerInfo(0);

            // Получение CN (полное имя).
            X509Certificate signerCert = signer.getSignerCertificate();
            String dn = signerCert.getSubjectDN().getName();
            String CN = getValByAttributeTypeFromIssuerDN(dn,"CN=");

            User user = usersService.loadUserByUsername(CN);
            if (!user.isEnabled()) {
                msgMap.put("error", "Пользователь не активирован!");
                logger.warn("Пользователь не активирован!");
                return new ResponseEntity<>(msgMap, HttpStatus.UNAUTHORIZED);
            }
            String userOGRNandINN = user.getOgrn() + " " + user.getInn();
            String signatureOGRN_INN = getOGRNAndINN(cAdESSignatureLong);
            if(userOGRNandINN.equalsIgnoreCase(signatureOGRN_INN) && user.isEnabled()){
                return new ResponseEntity<>("Bearer", HttpStatus.OK);
            } else {
                msgMap.put("error", "Ошибка авторизации");
                msgMap.put("UserID", user.getUserId());
                msgMap.put("DisplayName", user.getDisplayName());
                return new ResponseEntity<>(msgMap, HttpStatus.UNAUTHORIZED);
            }
        } catch (Exception e) {
            msgMap.put("error", "Пользователь не найден.");
            logger.warn("Пользователь не найден.");
            return new ResponseEntity<>(msgMap, HttpStatus.UNAUTHORIZED);
        }
    }

    /**
     * Метод для полчения CN (полное имя) из сертификата
     *
     * @param dn отличительное имя субъекта.
     * @param attributeType атрибут, который требуется найти в имени субъекта.
     */
    //
    private String getValByAttributeTypeFromIssuerDN(String dn, String attributeType)
    {
        String[] dnSplits = dn.split(",");
        for (String dnSplit : dnSplits)
        {
            if (dnSplit.contains(attributeType))
            {
                String[] cnSplits = dnSplit.trim().split("=");
                if(cnSplits[1]!= null)
                {
                    return cnSplits[1].trim();
                }
            }
        }
        return "";
    }
    
    @CrossOrigin(origins = "*")
    @PostMapping(value = "/validate")
    @ApiOperation(value = "Запрос на проверку подписи",
            notes = "ru.glab.docservice.controller.JwtAuthenticationController.validate",
            tags = "Аутентификация")
    private ResponseEntity<?> validate(@RequestBody String signature) throws Exception {

        logger.info("Вызван метод validate");

        // Включаем возможность онлайновой проверки.
        System.setProperty("com.ibm.security.enableCRLDP", "true");
        System.setProperty("com.sun.security.enableCRLDP", "true");
        System.setProperty("com.sun.security.enableAIAcaIssuers", "true");

        // Эксплуатация осуществяется путем добавления провайдеров в список java.security
        Security.addProvider(new JCP());
        Security.addProvider(new RevCheck());
        Security.addProvider(new CryptoProvider());
        Security.addProvider(new JCSP());


        // Исходная подпись в виде потока байтов из файла.
        byte[] buffer = signature.getBytes();
        byte[] decodingArray = Base64.decode(signature);

        // Декодирует совмещенную подпись с автоопределение типо.
        CAdESSignature cAdESSignatureLong = new CAdESSignature(decodingArray, null, null);

        // Валидация подписи и формирования информации: ОГРН и ИНН подписанта.
        String OGRN_INN = getOGRNAndINN(cAdESSignatureLong);
        if(OGRN_INN == null){
            logger.warn("ОГРН/ИНН подписанта не найдены;");
            throw new Exception("ОГРН/ИНН подписанта не найдены;");
        }

        if(isCADESvalidate(cAdESSignatureLong) & signTypeValid){
            resultValid = true;
            return new ResponseEntity<>("Подпись валидная;", HttpStatus.OK);
        }
        return new ResponseEntity<>("Подпись не прошла проверку;", HttpStatus.UNAUTHORIZED);
    }

    /**
     * Проверка на валидность подписи
     *
     * @param signature CAdES подпись.
     */
    private boolean isCADESvalidate(CAdESSignature signature) throws Exception {
        try
        {
            CollectionStore certs = signature.getCertificateStore();

            for (CAdESSigner cAdESSigner : signature.getCAdESSignerInfos()) {
                SignerInformation si = cAdESSigner.getSignerInfo();
                // получаем идентификатор сертификата, которым подписаны данные
                SignerId signerId = si.getSID();
                // ищем сертификат в коллекции по идентификатору
                Collection certCollection = certs.getMatches(signerId);
                // выбираем первый сертификат, здесь не должно быть больше одного
                Iterator certIt = certCollection.iterator();
                X509CertificateHolder certHolder = (X509CertificateHolder) certIt.next();
                // создаем Verifier на основании сертификата, Verifier использует публичный ключ сертификата
                SignerInformationVerifier verifier = new JcaSimpleSignerInfoVerifierBuilder().build(certHolder);
                // проверяем подпись
                return si.verify(verifier);
            }
        }
        catch (CertificateException | OperatorCreationException | CMSException e) {
            logger.warn("Подпись не прошла валидацию;");
            throw new Exception("Подпись не прошла валидацию:", e);
        }
        return false;
    }

    /**
     * Информация о подписи: ОГРН и ИНН подписанта.
     *
     * @param cAdESSignatureLong CAdES подпись.
     */
    private String getOGRNAndINN(CAdESSignature cAdESSignatureLong) throws Exception {
        try{
            // Дерево сертификата
            if(cAdESSignatureLong != null ) {
                ASN1Set asn1Set = getSignatureInfo(cAdESSignatureLong);
                if(asn1Set == null){
                    logger.warn("Невозможно получить атрибуты подписи;");
                    throw new Exception("Невозможно получить атрибуты подписи;");
                }
                DERSequence sequence = (DERSequence)
                        ((DERSequence)
                                ((DERSequence)
                                        ((DERSequence) (
                                                (DERSequence) asn1Set.getObjectAt(0)
                                        ).getObjectAt(0)
                                        ).getObjectAt(0)
                                ).getObjectAt(2)
                        ).getObjectAt(0);

                DERSequence sequence1 = (DERSequence)
                        ((DERTaggedObject) sequence.getObjectAt(0)
                        ).getObject();

                String OGRN = null;
                String INN = null;

                for (ASN1Encodable setItem : sequence1) {

                    DERSet subSet = (DERSet) setItem;
                    if (subSet == null)
                        continue;

                    ASN1ObjectIdentifier oid = (ASN1ObjectIdentifier)
                            ((DERSequence) subSet.getObjectAt(0)
                            ).getObjectAt(0);
                    if (oid == null) continue;

                    if (oid.getId().equalsIgnoreCase("1.2.643.100.1")) {
                        OGRN = (((DERSequence) subSet.getObjectAt(0)
                        ).getObjectAt(1)).toString();
//                        ru.CryptoPro.reprov.utils.Debug.println("ОГРН: %s", OGRN);
                    }
                    if (oid.getId().equalsIgnoreCase("1.2.643.3.131.1.1")) {
                        INN = (((DERSequence) subSet.getObjectAt(0)
                        ).getObjectAt(1)).toString();
//                        ru.CryptoPro.reprov.utils.Debug.println("ИНН: %s", INN);
                    }
                }

                if (OGRN == null) return null;
                if (INN == null) return null;
                return OGRN + " " + INN;
            }
            else {
                logger.warn("Подпись не создана;");
                throw new Exception("Подпись не создана;");
            }
        }catch (NullPointerException ex){
            logger.warn("Информация о подписи не получена;");
            throw new Exception("Информация о подписи не получена:", ex);
        }
    }

    /**
     * Информация о подписи.
     *
     * @param signature CAdES подпись.
     */
    private ASN1Set getSignatureInfo(CAdESSignature signature) throws Exception {
        // Список подписей.
        int signerIndex = 1;
        for (CAdESSigner signer : signature.getCAdESSignerInfos()) {
            return getSignerInfo(signer, signerIndex++);
        }
        return null;
    }

    /**
     * Валидация подписи и информация об отдельной подписи.
     *
     * @param signer Подпись.
     * @param index Индекс подписи.
     */
    private static ASN1Set getSignerInfo(CAdESSigner signer, int index) throws Exception {
        // Валидация подписи
        try {
            if (CAdESType.CAdES_X_Long_Type_1.equals(signer.getSignatureType())) {
                signTypeValid = signer.getSignerInfo().verify(new JcaSimpleSignerInfoVerifierBuilder().setProvider("BC")
                        .build(signer.getSignerCertificate().getPublicKey()));
                logger.warn("TypeValid : %b", signTypeValid);
            }
        } catch(Exception ex) {
            logger.warn("Подпись не прошла типовую проверку!");
            throw new Exception("Подпись не прошла типовую проверку:", ex);
        }
        // Получение содержимого таблицы аттрибутов.
        return getSignerAttributeTableInfo(index,
                signer.getSignerSignedAttributes(), "signed");
    }

    /**
     * Содержимое таблицы аттрибутов.
     *
     * @param i Номер подписанта.
     * @param table Таблица с аттрибутами.
     * @param type Тип таблицы: "signed".
     */
    private static ASN1Set getSignerAttributeTableInfo(int i, AttributeTable table,
                                                       String type) {
        if (table == null) {
            return null;
        } // if

        Hashtable attributes = table.toHashtable();
        Enumeration attributesEnum = attributes.elements();

        while (attributesEnum.hasMoreElements()) {
            Attribute attribute = Attribute.getInstance(attributesEnum.nextElement());
            if(attribute.getAttrType().getId().equalsIgnoreCase("1.2.840.113549.1.9.16.2.47")){
                return attribute.getAttrValues();
            }
        } // while
        return null;
    }
}