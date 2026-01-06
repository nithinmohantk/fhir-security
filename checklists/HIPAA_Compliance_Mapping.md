# HIPAA Compliance Mapping for FHIR APIs

**Version:** 1.0  
**Last Updated:** January 2026  
**Author:** Nithin Mohan T K  
**Repository:** https://github.com/nithinmohantk/fhir-security

---

## Overview

This document maps HIPAA Security Rule requirements (45 CFR Part 164) to FHIR API implementations. It provides specific technical controls and FHIR configurations to satisfy each requirement.

**Applicability:** All covered entities and business associates handling Protected Health Information (PHI) in the United States.

**Penalty Tiers:**
- Tier 1 (Unknown): $100 - $50,000 per violation
- Tier 2 (Reasonable Cause): $1,000 - $50,000 per violation
- Tier 3 (Willful Neglect, Corrected): $10,000 - $50,000 per violation
- Tier 4 (Willful Neglect, Not Corrected): $50,000 per violation
- Maximum annual penalty: $1.5 million per violation category

---

## 1. Administrative Safeguards (§164.308)

### 1.1 Security Management Process (§164.308(a)(1))

#### Risk Analysis (Required)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Conduct accurate and thorough risk assessment | Document all FHIR endpoints, data flows, and access points | ☐ |
| Identify threats to PHI confidentiality, integrity, availability | Threat model for FHIR API (STRIDE analysis) | ☐ |
| Assess current security measures | Inventory OAuth scopes, encryption, access controls | ☐ |
| Determine likelihood and impact of threats | Risk matrix for each FHIR resource type | ☐ |

#### Risk Management (Required)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Implement security measures to reduce risk | FAPI 2.0, DPoP, mTLS, encryption | ☐ |
| Document risk decisions | Risk acceptance documentation | ☐ |
| Annual risk assessment review | Update threat model annually | ☐ |

#### Sanction Policy (Required)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Apply sanctions for policy violations | Document disciplinary actions for API misuse | ☐ |
| Document policy violations | Log unauthorized access attempts | ☐ |

#### Information System Activity Review (Required)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Regularly review audit logs | Daily/weekly FHIR API log review | ☐ |
| Review access reports | OAuth token usage reports | ☐ |
| Review security incidents | FHIR-related security event analysis | ☐ |

**FHIR Audit Logging:**
```json
{
  "resourceType": "AuditEvent",
  "type": {
    "system": "http://dicom.nema.org/resources/ontology/DCM",
    "code": "110112",
    "display": "Query"
  },
  "action": "R",
  "recorded": "2025-01-15T10:30:00Z",
  "outcome": "0",
  "agent": [{
    "who": { "reference": "Practitioner/12345" },
    "requestor": true
  }],
  "source": {
    "observer": { "reference": "Device/fhir-server-1" }
  },
  "entity": [{
    "what": { "reference": "Patient/67890" },
    "type": {
      "system": "http://terminology.hl7.org/CodeSystem/audit-entity-type",
      "code": "1"
    }
  }]
}
```

### 1.2 Assigned Security Responsibility (§164.308(a)(2)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Identify security official | Document FHIR API security owner | ☐ |
| Define security responsibilities | Role description includes API security | ☐ |
| Security official has authority | Authority to enforce OAuth policies | ☐ |

### 1.3 Workforce Security (§164.308(a)(3))

#### Authorization/Supervision (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Implement procedures for workforce access | SMART scopes define access levels | ☐ |
| Supervise workforce PHI access | Audit logs reviewed by supervisor | ☐ |

#### Workforce Clearance (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Background checks before PHI access | HR verification before OAuth client creation | ☐ |
| Access granted based on job function | Scope assignment matches role | ☐ |

#### Termination Procedures (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Revoke access on termination | OAuth client/token revocation process | ☐ |
| Retrieve access credentials | Disable FHIR API credentials | ☐ |
| Change access codes | Rotate API keys on termination | ☐ |

### 1.4 Information Access Management (§164.308(a)(4))

#### Access Authorization (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Implement access authorization policies | SMART scope approval workflow | ☐ |
| Document access authorization | OAuth client registration records | ☐ |

#### Access Establishment and Modification (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Procedures for access provisioning | FHIR access request process | ☐ |
| Procedures for access modification | Scope change request process | ☐ |
| Document access changes | Audit trail of scope modifications | ☐ |

### 1.5 Security Awareness and Training (§164.308(a)(5))

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Security reminders (Addressable) | Regular API security bulletins | ☐ |
| Protection from malicious software (Addressable) | Secure coding training | ☐ |
| Log-in monitoring (Addressable) | Failed OAuth authentication alerts | ☐ |
| Password management (Addressable) | API key/secret management training | ☐ |

### 1.6 Security Incident Procedures (§164.308(a)(6)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Identify security incidents | FHIR API anomaly detection | ☐ |
| Respond to security incidents | FHIR breach response playbook | ☐ |
| Mitigate harmful effects | Token revocation, IP blocking procedures | ☐ |
| Document incidents and outcomes | Incident records for FHIR-related events | ☐ |

### 1.7 Contingency Plan (§164.308(a)(7))

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Data backup plan (Required) | FHIR database backup procedures | ☐ |
| Disaster recovery plan (Required) | FHIR server failover procedures | ☐ |
| Emergency mode operation plan (Required) | FHIR API degraded mode procedures | ☐ |
| Testing and revision (Addressable) | Annual DR testing for FHIR systems | ☐ |
| Applications and data criticality (Addressable) | FHIR resource prioritization | ☐ |

### 1.8 Evaluation (§164.308(a)(8)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Periodic technical evaluation | Annual FHIR API penetration testing | ☐ |
| Evaluate against specifications | FHIR conformance testing | ☐ |
| Evaluate environmental changes | Re-assess when FHIR version changes | ☐ |

### 1.9 Business Associate Contracts (§164.308(b)(1)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Written BAA with business associates | BAAs for all FHIR service providers | ☐ |
| BAA requires safeguards | BAA specifies FHIR security requirements | ☐ |
| BAA requires breach notification | 60-day breach notification in BAA | ☐ |

---

## 2. Physical Safeguards (§164.310)

### 2.1 Facility Access Controls (§164.310(a)(1))

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Contingency operations (Addressable) | Physical access to backup FHIR servers | ☐ |
| Facility security plan (Addressable) | Data center hosting FHIR servers secured | ☐ |
| Access control and validation (Addressable) | Visitor logs for FHIR infrastructure | ☐ |
| Maintenance records (Addressable) | Hardware maintenance logs | ☐ |

### 2.2 Workstation Use (§164.310(b)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Specify workstation functions | Define approved FHIR client workstations | ☐ |
| Physical attributes of workstations | Screen locks, privacy screens | ☐ |

### 2.3 Workstation Security (§164.310(c)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Restrict physical access to workstations | Secure developer workstations | ☐ |

### 2.4 Device and Media Controls (§164.310(d)(1))

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Disposal (Required) | Secure disposal of FHIR database storage | ☐ |
| Media re-use (Required) | Sanitization before re-use | ☐ |
| Accountability (Addressable) | Hardware asset inventory | ☐ |
| Data backup and storage (Addressable) | Encrypted backup storage | ☐ |

---

## 3. Technical Safeguards (§164.312)

### 3.1 Access Control (§164.312(a)(1))

#### Unique User Identification (Required)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Assign unique identifier to each user | OAuth subject (sub) claim unique | ☐ |
| User ID traceable in audit logs | Practitioner/Patient reference in AuditEvent | ☐ |

**FHIR Implementation:**
```
OAuth Token Claims:
{
  "sub": "practitioner-12345",
  "fhirUser": "https://fhir.example.com/Practitioner/12345"
}
```

#### Emergency Access Procedure (Required)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Procedures for emergency PHI access | Break-glass FHIR access procedure | ☐ |
| Document emergency access | AuditEvent with emergency code | ☐ |

**Break-Glass Implementation:**
```json
{
  "resourceType": "AuditEvent",
  "type": {
    "code": "110113",
    "display": "Security Alert"
  },
  "purposeOfEvent": [{
    "coding": [{
      "system": "http://terminology.hl7.org/CodeSystem/v3-ActReason",
      "code": "BTG",
      "display": "Break The Glass"
    }]
  }]
}
```

#### Automatic Logoff (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Terminate sessions after inactivity | OAuth access token lifetime ≤ 60 min | ☐ |
| Session timeout for FHIR clients | Refresh token rotation enabled | ☐ |

#### Encryption and Decryption (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Encrypt ePHI as appropriate | FHIR database encryption (AES-256) | ☐ |
| Key management procedures | HSM/KMS for encryption keys | ☐ |

### 3.2 Audit Controls (§164.312(b)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Hardware/software audit mechanisms | FHIR AuditEvent resources created | ☐ |
| Procedures for audit log review | Daily security log review | ☐ |
| Audit log retention | 6-year retention for AuditEvent | ☐ |

**Required Audit Log Fields:**
| Field | FHIR AuditEvent Element | Example |
|-------|------------------------|---------|
| Who | agent.who | Practitioner/12345 |
| What | entity.what | Patient/67890 |
| When | recorded | 2025-01-15T10:30:00Z |
| Where | source.observer | Device/fhir-server-1 |
| Why | purposeOfEvent | TREAT |
| Outcome | outcome | 0 (success) |

### 3.3 Integrity (§164.312(c)(1))

#### Mechanism to Authenticate ePHI (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Verify ePHI hasn't been altered | FHIR resource versioning (_history) | ☐ |
| Digital signatures where appropriate | FHIR Signature datatype | ☐ |
| Hash validation | ETags for resource integrity | ☐ |

**FHIR Integrity Controls:**
```
GET /fhir/Patient/12345
Response Headers:
  ETag: W/"3"
  Last-Modified: 2025-01-15T10:30:00Z
  
Resource:
  "meta": {
    "versionId": "3",
    "lastUpdated": "2025-01-15T10:30:00Z"
  }
```

### 3.4 Person or Entity Authentication (§164.312(d)) - Required

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Verify identity before access | OAuth 2.0 authentication | ☐ |
| Multi-factor authentication | MFA for clinician access | ☐ |
| Strong authentication for API access | DPoP or mTLS for machine access | ☐ |

**Authentication Strength Matrix:**
| Access Type | Minimum Auth | Recommended |
|-------------|--------------|-------------|
| Patient Portal | Password + MFA | FIDO2/WebAuthn |
| Clinician App | Password + MFA | Smart Card + PIN |
| Machine-to-Machine | Client Credentials + DPoP | mTLS + DPoP |
| Bulk Export | System Scope + DPoP | mTLS + Short Token |

### 3.5 Transmission Security (§164.312(e)(1))

#### Integrity Controls (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Protect ePHI integrity in transit | TLS 1.2+ for all FHIR APIs | ☐ |
| Detect unauthorized modification | TLS record MAC validation | ☐ |

#### Encryption (Addressable)
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Encrypt ePHI in transit | TLS encryption for all FHIR traffic | ☐ |
| Strong cipher suites | TLS_AES_256_GCM_SHA384 preferred | ☐ |
| No deprecated protocols | TLS 1.0/1.1 disabled | ☐ |

**TLS Configuration:**
```nginx
ssl_protocols TLSv1.2 TLSv1.3;
ssl_ciphers 'TLS_AES_256_GCM_SHA384:TLS_CHACHA20_POLY1305_SHA256:ECDHE-ECDSA-AES256-GCM-SHA384';
ssl_prefer_server_ciphers on;
```

---

## 4. Organizational Requirements (§164.314)

### 4.1 Business Associate Contracts
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Written contract with BA | BAA template for FHIR services | ☐ |
| BA safeguards documented | FHIR security requirements in BAA | ☐ |
| BA reporting requirements | Breach notification within 60 days | ☐ |
| BA subcontractor requirements | Flow-down of FHIR security requirements | ☐ |

### 4.2 Group Health Plan Requirements
| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Plan document amendments | FHIR access provisions in plan docs | ☐ |
| Certification of safeguards | Annual FHIR security certification | ☐ |

---

## 5. Policies and Procedures (§164.316)

### 5.1 Documentation Requirements

| Requirement | FHIR Implementation | Status |
|-------------|---------------------|--------|
| Written policies and procedures | FHIR API security policies documented | ☐ |
| Policies accessible to workforce | Policy portal includes FHIR policies | ☐ |
| Documentation retained 6 years | Policy version control | ☐ |
| Policies updated as needed | Annual policy review includes FHIR | ☐ |

**Required FHIR Security Policies:**
1. FHIR API Access Control Policy
2. OAuth Token Management Policy
3. FHIR Audit Logging Policy
4. FHIR Incident Response Procedures
5. FHIR Business Associate Management
6. FHIR Encryption Standards
7. FHIR Bulk Export Security Policy

---

## Sign-Off

### HIPAA Security Rule Compliance

| Role | Name | Signature | Date |
|------|------|-----------|------|
| HIPAA Security Officer | | | |
| CIO/CISO | | | |
| Compliance Officer | | | |
| Privacy Officer | | | |

### Compliance Status

- [ ] All Required standards implemented
- [ ] Addressable standards evaluated and documented
- [ ] Risk assessment completed within last 12 months
- [ ] BAAs signed with all business associates
- [ ] Policies reviewed and updated
- [ ] Workforce training completed

**Compliance Status:** ☐ COMPLIANT / ☐ PARTIAL / ☐ NON-COMPLIANT

**Last Risk Assessment:** _____________

**Next Review Date:** _____________

---

*This document should be reviewed annually and updated when significant changes occur to FHIR systems or applicable regulations.*
