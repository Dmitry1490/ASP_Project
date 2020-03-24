package ru.glab.docservice.controller;

import io.swagger.annotations.Api;
import io.swagger.annotations.ApiOperation;
import org.bouncycastle.asn1.*;
import org.bouncycastle.asn1.cms.Attribute;
import org.bouncycastle.asn1.cms.AttributeTable;
import org.bouncycastle.asn1.x500.AttributeTypeAndValue;
import org.bouncycastle.asn1.x500.RDN;
import org.bouncycastle.cert.X509CertificateHolder;
import org.bouncycastle.cms.SignerInformation;
import org.bouncycastle.cms.jcajce.JcaSimpleSignerInfoVerifierBuilder;
import org.bouncycastle.tsp.TimeStampToken;
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
import ru.CryptoPro.CAdES.CAdESSignerXLT1;
import ru.CryptoPro.CAdES.CAdESType;
import ru.CryptoPro.Crypto.CryptoProvider;
import ru.CryptoPro.JCP.JCP;
import ru.CryptoPro.JCP.tools.Array;
import ru.CryptoPro.JCSP.JCSP;
import ru.CryptoPro.reprov.RevCheck;
import ru.glab.docservice.config.CadesConfiguration;
import ru.glab.docservice.config.JwtTokenUtil;
import ru.glab.docservice.model.CurrentUser;
import ru.glab.docservice.model.JwtRequest;
import ru.glab.docservice.model.postgres.User;
import ru.glab.docservice.service.postgres.UsersService;

import java.io.FileInputStream;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.security.*;
import java.security.cert.*;
import java.security.cert.Certificate;
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

    private static final Logger logger = LoggerFactory.getLogger(ru.glab.docservice.controller.JwtAuthenticationController.class);


    private Map<String, Object> msgMap = new HashMap<>();

    private static boolean resultValidType = false;
    private static boolean resultValidOGRNandINN = false;


    @CrossOrigin(origins = "*")
    @PostMapping(value = AUTHENTICATE_URL)
    @ApiOperation(value = "Запрос на создание токена",
            notes = "ru.glab.docservice.controller.JwtAuthenticationController.createAuthenticationToken",
            tags = "Аутентификация")
    public ResponseEntity<?> createAuthenticationToken(@RequestBody JwtRequest authenticationRequest) {
        msgMap.clear();
        logger.info("Вызван метод createAuthenticationToken");

        if (authenticationRequest == null || authenticationRequest.getUsername().length() == 0 ||
                authenticationRequest.getPassword().length() == 0) { //Проверяем валидность логина и пароля
            msgMap.put("error","Пустые логин или пароль!");
            logger.warn("Пустые логин или пароль!");
            return new ResponseEntity<>(msgMap, HttpStatus.BAD_REQUEST);
        }
        try {
            final User user = usersService
                    .loadUserByUsername(authenticationRequest.getUsername());
            if (!user.isEnabled()) {
                msgMap.put("error", "Пользователь не активирован!");
                logger.warn("Пользователь не активирован!");
                return new ResponseEntity<>(msgMap, HttpStatus.UNAUTHORIZED);
            }
            try {
                authenticate(authenticationRequest.getUsername(), authenticationRequest.getPassword());
            } catch (Exception e) {
                msgMap.put("error", e.getMessage());
                logger.error(e.getMessage());
                return new ResponseEntity<>(msgMap, HttpStatus.UNAUTHORIZED);
            }
            final String token = jwtTokenUtil.generateToken(user);
            msgMap.put("token", token);
            msgMap.put("userId", user.getUserId());
            msgMap.put("rolesList", user.getUserRolesInfo());
            msgMap.put("fullName", user.getDisplayName());
            CurrentUser.setDisplayName(user.getDisplayName());
            CurrentUser.setUserId(user.getUserId());
            CurrentUser.setDisplayName(user.getDisplayName());
            return new ResponseEntity<>(msgMap, HttpStatus.OK);
        }
        catch (UsernameNotFoundException e) {
            msgMap.put("error", "Неверный логин или пароль!");
            logger.warn("Неверный логин или пароль!");
            return new ResponseEntity<>(msgMap, HttpStatus.UNAUTHORIZED);
        }
    }

    private void authenticate(String username, String password) throws Exception {
        try {
            authenticationManager.authenticate(new UsernamePasswordAuthenticationToken(username, password));
        } catch (DisabledException e) {
            throw new Exception("Пользователь не активирован!", e);
        } catch (BadCredentialsException e) {
            throw new Exception("Неверный логин или пароль!", e);
        }
    }

    @CrossOrigin(origins = "*")
    @PostMapping(value = "/validate")
    private void validate() throws Exception {
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
        byte[] array = Files.readAllBytes(Paths.get("C:\\Users\\Дмитрий\\Desktop\\SignCryptoPro\\ИНН+ОГРН.sig"));
        byte[] decodingArray = Base64.decode(array);

        // Декодирует совмещенную подпись с автоопределение типо.
        CAdESSignature cAdESSignatureLong = new CAdESSignature(decodingArray, null, null);

        // Валидация подписи и формирования информации: ОГРН и ИНН подписанта.
        String OGRN_INN = getOGRNAndINN(cAdESSignatureLong);
        if(OGRN_INN == null) System.out.println("ОГРН или ИНН подписанта не найдено");
        else {
            System.out.println("ОГРН и ИНН: " + OGRN_INN);
            resultValidOGRNandINN = isResultValidOGRNandINN(cAdESSignatureLong, OGRN_INN);
        }
        //Резульатат валидации;
        if(resultValidType & resultValidOGRNandINN) {
            System.out.println("Подпись валидная");}
        else System.out.println("Подпись невалидная");

        System.out.println("INFO-END!");
    }

    /**
     * Проверка на валидность ОГРН и ИНН подписанта;
     *
     * @param cAdESSignatureLong CAdES подпись.
     * @param OGRNandINN ОГРН и ИНН подписанта;
     */
    private boolean isResultValidOGRNandINN(CAdESSignature cAdESSignatureLong, String OGRNandINN){
        boolean OGRNValid = false;
        boolean INNVAlid = false;
        String[] split = OGRNandINN.split(" ");
        String OGRN = split[0];
        String INN = split[1];

        CollectionStore signatureCertificates = cAdESSignatureLong.getCertificateStore();
        for (Object certificates: signatureCertificates
        ) {
            //Если OGRNValid и INNVAlid true, то ОГРН и ИНН уже были найдены в одном из сертификатов в CollectionStore;
            if (OGRNValid & INNVAlid) break;
            // Если OGRNValid или INNVAlid false, то продолжаем искать ОГРН и ИНН в CollectionStore;
            X509CertificateHolder x509CertificateHolder = (X509CertificateHolder) certificates;
            RDN[] rdnsOGRN = x509CertificateHolder.getIssuer().getRDNs(new ASN1ObjectIdentifier("1.2.643.100.1"));
            RDN[] rdnsINN = x509CertificateHolder.getIssuer().getRDNs(new ASN1ObjectIdentifier("1.2.643.3.131.1.1"));
            if(rdnsOGRN.length > 0){
                OGRNValid = (rdnsOGRN[0].getTypesAndValues())[0].getValue().toString().equals(OGRN);
            } else {
                System.out.println("ОГРН в сертификате не найден");
                continue;
            }
            if(rdnsINN.length > 0){
                INNVAlid = (rdnsINN[0].getTypesAndValues())[0].getValue().toString().equals(INN);
            } else {
                System.out.println("ИНН в сертификате не найден");
                continue;
            }
        }
        if(OGRNValid & INNVAlid) return true;
        else return false;
    }

    /**
     * Информация о подписи: ОГРН и ИНН подписанта.
     *
     * @param cAdESSignatureLong CAdES подпись.
     */
    private String getOGRNAndINN(CAdESSignature cAdESSignatureLong){
        try{
            // Дерево сертификата
            if(cAdESSignatureLong != null ) {
                ASN1Set asn1Set = getSignatureInfo(cAdESSignatureLong);
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
                        ru.CryptoPro.reprov.utils.Debug.println("ОГРН: %s", OGRN);
                    }
                    if (oid.getId().equalsIgnoreCase("1.2.643.3.131.1.1")) {
                        INN = (((DERSequence) subSet.getObjectAt(0)
                        ).getObjectAt(1)).toString();
                        ru.CryptoPro.reprov.utils.Debug.println("ИНН: %s", INN);
                    }
                }

                if (OGRN == null) return null;
                if (INN == null) return null;
                return OGRN + " " + INN;
            }
            else {
                System.out.println("Подпись не создана");
                return null;
            }


        }catch (NullPointerException ex){
            return "Информация о подписи не получена";
        }
    }

    /**
     * Информация о подписи: кто подписал, тип подписи, штампы времени.
     *
     * @param signature CAdES подпись.
     */
    private ASN1Set getSignatureInfo(CAdESSignature signature) {
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
    private static ASN1Set getSignerInfo(CAdESSigner signer, int index) {
        // Валидация подписи
        try {
            if (CAdESType.CAdES_X_Long_Type_1.equals(signer.getSignatureType())) {
                resultValidType = signer.getSignerInfo().verify(new JcaSimpleSignerInfoVerifierBuilder().setProvider("BC")
                        .build(signer.getSignerCertificate().getPublicKey()));
                System.out.printf("valid : %b \n", resultValidType);
            }
        } catch(Exception ex) {
            System.out.println("valid : false");
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
     * @param type Тип таблицы: "signed" или "unsigned".
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