# GDPR Compliance Checklist for FHIR APIs

**Version:** 1.0  
**Last Updated:** January 2026  
**Author:** Nithin Mohan T K  
**Repository:** https://github.com/nithinmohantk/fhir-security

---

## Overview

This checklist maps GDPR (General Data Protection Regulation) requirements to FHIR API implementations. It covers technical controls, organizational measures, and documentation requirements for healthcare organizations processing EU resident data.

**Applicability:** Required if you process personal data of EU residents, regardless of where your organization is located.

**Severity Levels:**
- 🔴 **MANDATORY** - Legal requirement, non-compliance = potential fines
- 🟠 **RECOMMENDED** - Best practice for demonstrating compliance
- 🟡 **OPTIONAL** - Enhanced protection

**Potential Fines:** Up to €20 million or 4% of annual global turnover (whichever is higher)

---

## 1. Lawful Basis for Processing (Article 6)

### 1.1 Legal Basis Documentation
| Item | Article | Status | Implementation Notes |
|------|---------|--------|---------------------|
| Documented legal basis for each processing activity | Art. 6(1) | ☐ | Consent, contract, legal obligation, vital interests, public task, or legitimate interests |
| Legal basis recorded in processing register | Art. 30 | ☐ | Include in Records of Processing Activities (ROPA) |
| Legal basis communicated in privacy notice | Art. 13/14 | ☐ | Must be provided at data collection |

### 1.2 FHIR-Specific Implementation
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| FHIR Consent resource captures patient consent | 🔴 MANDATORY | ☐ | Map to `Consent.status`, `Consent.scope`, `Consent.category` |
| Consent timestamp recorded | 🔴 MANDATORY | ☐ | `Consent.dateTime` field populated |
| Consent scope defines permitted data access | 🔴 MANDATORY | ☐ | `Consent.provision.type` (permit/deny) |
| Consent withdrawal mechanism implemented | 🔴 MANDATORY | ☐ | API endpoint to update `Consent.status = inactive` |
| Consent linked to data subject (Patient) | 🔴 MANDATORY | ☐ | `Consent.patient` reference |

**FHIR Consent Resource Example:**
```json
{
  "resourceType": "Consent",
  "status": "active",
  "scope": {
    "coding": [{
      "system": "http://terminology.hl7.org/CodeSystem/consentscope",
      "code": "patient-privacy"
    }]
  },
  "category": [{
    "coding": [{
      "system": "http://loinc.org",
      "code": "59284-0",
      "display": "Patient Consent"
    }]
  }],
  "patient": { "reference": "Patient/12345" },
  "dateTime": "2025-01-15T10:30:00Z",
  "provision": {
    "type": "permit",
    "period": { "start": "2025-01-15", "end": "2026-01-15" },
    "purpose": [{
      "system": "http://terminology.hl7.org/CodeSystem/v3-ActReason",
      "code": "TREAT"
    }]
  }
}
```

---

## 2. Data Subject Rights (Articles 15-22)

### 2.1 Right of Access (Article 15)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Subject Access Request (SAR) endpoint | 🔴 MANDATORY | ☐ | `GET /fhir/Patient/{id}/$everything` or custom operation |
| Response within 30 days | 🔴 MANDATORY | ☐ | Automated tracking required |
| Machine-readable format available | 🔴 MANDATORY | ☐ | FHIR JSON/XML format satisfies this |
| Identity verification before access | 🔴 MANDATORY | ☐ | Strong authentication required |
| Audit log of SAR requests | 🟠 RECOMMENDED | ☐ | Track who requested what, when |
| Free of charge (first copy) | 🔴 MANDATORY | ☐ | May charge for additional copies |

**FHIR Implementation:**
```
GET /fhir/Patient/12345/$everything
Accept: application/fhir+json

Response: Bundle containing all patient-related resources
```

### 2.2 Right to Rectification (Article 16)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Mechanism to correct inaccurate data | 🔴 MANDATORY | ☐ | `PUT /fhir/Patient/{id}` with corrections |
| Mechanism to complete incomplete data | 🔴 MANDATORY | ☐ | FHIR PATCH or PUT operations |
| Third-party notification of corrections | 🔴 MANDATORY | ☐ | Notify recipients if data was shared |
| Audit trail of corrections | 🟠 RECOMMENDED | ☐ | FHIR `_history` maintains versions |
| Correction request tracking | 🟠 RECOMMENDED | ☐ | Task resource or ticketing system |

### 2.3 Right to Erasure / Right to be Forgotten (Article 17)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Erasure endpoint implemented | 🔴 MANDATORY | ☐ | `DELETE /fhir/Patient/{id}` with cascade |
| Cascading deletion of related resources | 🔴 MANDATORY | ☐ | Delete all resources in patient compartment |
| Backup data deletion procedures | 🔴 MANDATORY | ☐ | Must delete from backups (or document retention) |
| Third-party deletion notification | 🔴 MANDATORY | ☐ | Notify all recipients to delete |
| Legitimate retention exceptions documented | 🔴 MANDATORY | ☐ | Legal holds, statutory retention periods |
| Healthcare-specific retention rules | 🔴 MANDATORY | ☐ | Medical records often have mandatory retention (varies by jurisdiction) |

**⚠️ Healthcare Exception:**
Medical records often have statutory retention periods (e.g., 10 years in many EU countries). Document your policy for handling erasure requests that conflict with legal retention requirements.

**FHIR Implementation:**
```
DELETE /fhir/Patient/12345/$purge
X-Cascade: true

Note: Custom operation needed for full GDPR erasure
Standard FHIR DELETE only marks as deleted, may not purge
```

### 2.4 Right to Data Portability (Article 20)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Data export in structured format | 🔴 MANDATORY | ☐ | FHIR Bundle export |
| Machine-readable format (JSON/XML) | 🔴 MANDATORY | ☐ | Standard FHIR formats |
| Direct transfer to another controller | 🟠 RECOMMENDED | ☐ | `$export` with destination parameter |
| Bulk export capability | 🟠 RECOMMENDED | ☐ | FHIR Bulk Data Export (`$export`) |
| Standard format (FHIR R4+) | 🟠 RECOMMENDED | ☐ | Interoperability with other systems |

**FHIR Bulk Export:**
```
POST /fhir/Patient/12345/$export
Accept: application/fhir+ndjson
Prefer: respond-async

Poll status URL until complete, then download NDJSON files
```

### 2.5 Right to Restriction of Processing (Article 18)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Mechanism to flag restricted data | 🔴 MANDATORY | ☐ | FHIR security labels or `Patient.meta.tag` |
| Restricted data excluded from processing | 🔴 MANDATORY | ☐ | Query filters respect restriction flag |
| Restriction lifted only with consent | 🔴 MANDATORY | ☐ | Consent resource update required |
| Audit of access to restricted data | 🔴 MANDATORY | ☐ | Enhanced logging for restricted resources |

**FHIR Implementation:**
```json
{
  "resourceType": "Patient",
  "id": "12345",
  "meta": {
    "security": [{
      "system": "http://terminology.hl7.org/CodeSystem/v3-ActReason",
      "code": "GDPR_RESTRICTED",
      "display": "GDPR Processing Restricted"
    }]
  }
}
```

### 2.6 Right to Object (Article 21)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Objection mechanism implemented | 🔴 MANDATORY | ☐ | Update Consent resource to `deny` |
| Processing stops upon valid objection | 🔴 MANDATORY | ☐ | Immediate effect required |
| Direct marketing opt-out | 🔴 MANDATORY | ☐ | Absolute right, no exceptions |
| Research objection handling | 🟠 RECOMMENDED | ☐ | Consent.purpose filtering |

### 2.7 Automated Decision-Making (Article 22)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| AI/ML decisions flagged | 🔴 MANDATORY | ☐ | Log when automated decision affects patient |
| Human review available | 🔴 MANDATORY | ☐ | Mechanism to request human intervention |
| Explainability of AI decisions | 🟠 RECOMMENDED | ☐ | Store reasoning in `DiagnosticReport.conclusion` |
| Right to contest AI decision | 🔴 MANDATORY | ☐ | Appeal process documented |

---

## 3. Security of Processing (Article 32)

### 3.1 Technical Measures
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Encryption at rest | 🔴 MANDATORY | ☐ | AES-256 or equivalent |
| Encryption in transit | 🔴 MANDATORY | ☐ | TLS 1.2+ required, 1.3 recommended |
| Pseudonymization capability | 🟠 RECOMMENDED | ☐ | Replace identifiers with pseudonyms |
| Access controls (RBAC) | 🔴 MANDATORY | ☐ | SMART on FHIR scopes |
| Regular security testing | 🟠 RECOMMENDED | ☐ | Annual penetration testing |

### 3.2 Organizational Measures
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Security policies documented | 🔴 MANDATORY | ☐ | Information security policy |
| Staff training on GDPR | 🔴 MANDATORY | ☐ | Annual training, documented |
| Vendor security assessments | 🟠 RECOMMENDED | ☐ | DPIAs for processors |
| Incident response plan | 🔴 MANDATORY | ☐ | 72-hour breach notification |

---

## 4. Breach Notification (Articles 33-34)

### 4.1 Supervisory Authority Notification (Article 33)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Notification within 72 hours | 🔴 MANDATORY | ☐ | From awareness of breach |
| Breach documentation maintained | 🔴 MANDATORY | ☐ | Facts, effects, remediation |
| Nature of breach described | 🔴 MANDATORY | ☐ | Categories and approx. number |
| DPO contact details provided | 🔴 MANDATORY | ☐ | Include in notification |
| Consequences assessment | 🔴 MANDATORY | ☐ | Likely consequences described |
| Remediation measures documented | 🔴 MANDATORY | ☐ | Steps taken to address |

### 4.2 Data Subject Notification (Article 34)
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| High-risk breach notification | 🔴 MANDATORY | ☐ | When high risk to rights/freedoms |
| Clear, plain language | 🔴 MANDATORY | ☐ | Non-technical description |
| Recommendations provided | 🔴 MANDATORY | ☐ | Steps data subjects can take |
| Communication method documented | 🟠 RECOMMENDED | ☐ | Direct or public announcement |

---

## 5. Data Protection by Design and Default (Article 25)

### 5.1 Privacy by Design
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Data minimization in APIs | 🔴 MANDATORY | ☐ | Only return necessary fields (`_elements` parameter) |
| Purpose limitation enforced | 🔴 MANDATORY | ☐ | Scopes limit access by purpose |
| Storage limitation implemented | 🔴 MANDATORY | ☐ | Automated retention/deletion |
| Default privacy-protective settings | 🔴 MANDATORY | ☐ | Opt-in not opt-out |
| Anonymization/aggregation options | 🟠 RECOMMENDED | ☐ | For research/analytics |

### 5.2 FHIR-Specific Privacy Controls
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| `_elements` parameter supported | 🟠 RECOMMENDED | ☐ | Return only requested fields |
| `_summary` parameter supported | 🟠 RECOMMENDED | ☐ | Reduced data views |
| Security labels implemented | 🟠 RECOMMENDED | ☐ | `meta.security` for sensitivity |
| Sensitive data masking | 🟠 RECOMMENDED | ☐ | Mask SSN, address in responses |

**FHIR Data Minimization:**
```
GET /fhir/Patient/12345?_elements=name,birthDate,gender
GET /fhir/Patient/12345?_summary=true
```

---

## 6. Data Processing Agreements (Article 28)

### 6.1 Processor Requirements
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Written DPA with all processors | 🔴 MANDATORY | ☐ | Controllers must have contracts |
| Subject matter and duration specified | 🔴 MANDATORY | ☐ | Clear scope |
| Nature and purpose of processing | 🔴 MANDATORY | ☐ | What and why |
| Type of personal data specified | 🔴 MANDATORY | ☐ | Categories of data |
| Categories of data subjects | 🔴 MANDATORY | ☐ | Patients, practitioners, etc. |
| Controller obligations documented | 🔴 MANDATORY | ☐ | Instructions documented |
| Processor obligations documented | 🔴 MANDATORY | ☐ | Security, confidentiality, etc. |
| Sub-processor approval required | 🔴 MANDATORY | ☐ | Prior authorization |
| Return/deletion on termination | 🔴 MANDATORY | ☐ | Data handling at contract end |

### 6.2 Cloud Provider DPAs
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Azure DPA signed | 🔴 MANDATORY | ☐ | Microsoft DPA/SCCs |
| AWS DPA signed | 🔴 MANDATORY | ☐ | AWS DPA |
| Google Cloud DPA signed | 🔴 MANDATORY | ☐ | Google Cloud DPA |
| Standard Contractual Clauses (SCCs) | 🔴 MANDATORY | ☐ | Required for non-EU transfers |

---

## 7. International Data Transfers (Article 44-49)

### 7.1 Transfer Mechanisms
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Adequacy decision check | 🔴 MANDATORY | ☐ | Is destination country adequate? |
| Standard Contractual Clauses (SCCs) | 🔴 MANDATORY | ☐ | If no adequacy decision |
| Binding Corporate Rules (BCRs) | 🟠 RECOMMENDED | ☐ | For multinational groups |
| Transfer Impact Assessment (TIA) | 🔴 MANDATORY | ☐ | Required for SCCs |
| Supplementary measures documented | 🟠 RECOMMENDED | ☐ | Additional protections |

### 7.2 FHIR Implementation for Transfers
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Data residency controls | 🔴 MANDATORY | ☐ | Geo-location of storage |
| Cross-border API restrictions | 🟠 RECOMMENDED | ☐ | Block requests from outside EU |
| Transfer logging | 🔴 MANDATORY | ☐ | Log all cross-border data flows |
| End-to-end encryption for transfers | 🟠 RECOMMENDED | ☐ | Additional protection layer |

---

## 8. Records of Processing Activities (Article 30)

### 8.1 Controller ROPA Requirements
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| Name and contact of controller | 🔴 MANDATORY | ☐ | Including DPO |
| Purposes of processing | 🔴 MANDATORY | ☐ | Each processing activity |
| Categories of data subjects | 🔴 MANDATORY | ☐ | Patients, staff, etc. |
| Categories of personal data | 🔴 MANDATORY | ☐ | Health data, identifiers, etc. |
| Categories of recipients | 🔴 MANDATORY | ☐ | Who receives the data |
| Third country transfers | 🔴 MANDATORY | ☐ | With safeguards |
| Retention periods | 🔴 MANDATORY | ☐ | For each category |
| Security measures description | 🔴 MANDATORY | ☐ | Technical and organizational |

### 8.2 FHIR Resource Mapping for ROPA
| ROPA Element | FHIR Resource | Notes |
|--------------|---------------|-------|
| Data subjects | Patient, Practitioner, RelatedPerson | Categories |
| Personal data | All clinical resources | ResourceType list |
| Recipients | Organization, Endpoint | Data sharing targets |
| Processing purposes | Consent.provision.purpose | ActReason codes |
| Retention | None (custom extension) | Add `retentionPeriod` extension |

---

## 9. Data Protection Impact Assessment (Article 35)

### 9.1 DPIA Requirements
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| DPIA conducted for high-risk processing | 🔴 MANDATORY | ☐ | Health data is always high-risk |
| Systematic description of processing | 🔴 MANDATORY | ☐ | Operations, scope, context |
| Necessity and proportionality assessed | 🔴 MANDATORY | ☐ | Is processing necessary? |
| Risks to data subjects identified | 🔴 MANDATORY | ☐ | Likelihood and severity |
| Risk mitigation measures documented | 🔴 MANDATORY | ☐ | Controls to reduce risk |
| DPO consulted | 🟠 RECOMMENDED | ☐ | DPO opinion included |
| DPIA reviewed annually | 🟠 RECOMMENDED | ☐ | Or when processing changes |

### 9.2 FHIR-Specific DPIA Considerations
| Processing Activity | Risk Level | Mitigation |
|--------------------|------------|------------|
| Patient data storage | HIGH | Encryption, access controls |
| Cross-organization sharing | HIGH | Consent, audit logs, SCCs |
| AI/ML clinical decision support | HIGH | Explainability, human review |
| Bulk data export | HIGH | Authorization, logging, encryption |
| Real-time event streaming (Kafka) | HIGH | Encryption, access controls, DPoP |
| Mobile app access | MEDIUM | Strong auth, certificate pinning |

---

## 10. Data Protection Officer (Article 37-39)

### 10.1 DPO Requirements
| Item | Severity | Status | Implementation Notes |
|------|----------|--------|---------------------|
| DPO appointed (if required) | 🔴 MANDATORY | ☐ | Public body or large-scale health data |
| DPO contact published | 🔴 MANDATORY | ☐ | Privacy policy, website |
| DPO contact registered with SA | 🔴 MANDATORY | ☐ | Supervisory authority notification |
| DPO independence ensured | 🔴 MANDATORY | ☐ | No instructions on tasks |
| DPO resources adequate | 🔴 MANDATORY | ☐ | Time, training, access |
| DPO involved in all DPA issues | 🔴 MANDATORY | ☐ | Early involvement |

---

## Sign-Off

### GDPR Compliance Approval

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Data Protection Officer | | | |
| Chief Information Security Officer | | | |
| Legal Counsel | | | |
| Chief Medical Officer | | | |
| CEO/Managing Director | | | |

### Compliance Status

- [ ] All MANDATORY items completed
- [ ] DPIA conducted and documented
- [ ] ROPA maintained and current
- [ ] DPO appointed and registered
- [ ] DPAs signed with all processors
- [ ] Breach notification procedures tested

**Overall Compliance Status:** ☐ COMPLIANT / ☐ PARTIAL / ☐ NON-COMPLIANT

**Next Review Date:** _____________

---

## Appendix A: GDPR Article Quick Reference

| Article | Topic | Key Requirement |
|---------|-------|-----------------|
| 5 | Principles | Lawfulness, fairness, transparency |
| 6 | Lawful Basis | One of six legal bases required |
| 7 | Conditions for Consent | Freely given, specific, informed |
| 9 | Special Categories | Health data requires explicit consent or exception |
| 12 | Transparent Information | Clear, plain language communication |
| 13-14 | Information to Provide | Privacy notices |
| 15 | Right of Access | SAR within 30 days |
| 16 | Right to Rectification | Correct inaccurate data |
| 17 | Right to Erasure | Right to be forgotten |
| 18 | Right to Restriction | Limit processing |
| 20 | Right to Portability | Export in machine-readable format |
| 21 | Right to Object | Stop processing |
| 22 | Automated Decisions | Profiling limitations |
| 25 | Privacy by Design | Built-in data protection |
| 28 | Processor Obligations | Written DPA required |
| 30 | Records of Processing | Maintain ROPA |
| 32 | Security of Processing | Appropriate technical/org measures |
| 33 | Breach Notification (SA) | 72 hours |
| 34 | Breach Notification (DS) | If high risk |
| 35 | DPIA | For high-risk processing |
| 37-39 | DPO | Appointment requirements |
| 44-49 | International Transfers | Safeguards required |

---

## Appendix B: Supervisory Authority Contacts

| Country | Supervisory Authority | Website |
|---------|----------------------|---------|
| EU (EDPB) | European Data Protection Board | edpb.europa.eu |
| Germany | BfDI | bfdi.bund.de |
| France | CNIL | cnil.fr |
| UK | ICO | ico.org.uk |
| Ireland | DPC | dataprotection.ie |
| Netherlands | AP | autoriteitpersoonsgegevens.nl |
| Spain | AEPD | aepd.es |
| Italy | Garante | garanteprivacy.it |

---

*Document Version Control: Track all changes in your organization's document management system.*

*Last reviewed: [DATE] by [NAME]*
