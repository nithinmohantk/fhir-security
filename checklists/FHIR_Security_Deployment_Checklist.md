# FHIR API Security Deployment Checklist

**Version:** 1.0  
**Last Updated:** January 2026  
**Author:** Nithin Mohan T K  
**Repository:** https://github.com/nithinmohantk/fhir-security

---

## Overview

This checklist covers 60+ validation points across 8 security domains for FHIR API production deployment. Complete all items before go-live.

**Severity Levels:**
- 🔴 **CRITICAL** - Must complete before deployment
- 🟠 **HIGH** - Should complete, can deploy with documented risk
- 🟡 **MEDIUM** - Recommended, complete within 30 days post-deployment
- 🟢 **LOW** - Best practice, complete within 90 days

---

## 1. Network Security

### 1.1 Transport Layer Security
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| TLS 1.3 enforced on all endpoints | 🔴 CRITICAL | ☐ | TLS 1.2 minimum, 1.3 preferred |
| TLS 1.0/1.1 disabled | 🔴 CRITICAL | ☐ | Vulnerable to POODLE, BEAST |
| Certificate from trusted CA | 🔴 CRITICAL | ☐ | No self-signed in production |
| Certificate pinning (mobile apps) | 🟠 HIGH | ☐ | Prevents MITM attacks |
| HSTS enabled (1 year, includeSubDomains) | 🟠 HIGH | ☐ | `max-age=31536000` |
| Certificate expiry monitoring | 🔴 CRITICAL | ☐ | Alert 90/30/7 days before |

### 1.2 Network Architecture
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| API Gateway deployed | 🔴 CRITICAL | ☐ | Kong, AWS API Gateway, Azure APIM |
| WAF configured | 🔴 CRITICAL | ☐ | OWASP rules enabled |
| DDoS protection enabled | 🟠 HIGH | ☐ | AWS Shield, Cloudflare, Azure DDoS |
| Network segmentation implemented | 🟠 HIGH | ☐ | FHIR server in private subnet |
| IP whitelisting for admin access | 🔴 CRITICAL | ☐ | VPN or bastion host required |
| Rate limiting configured | 🔴 CRITICAL | ☐ | Per-client and global limits |

---

## 2. Authentication

### 2.1 OAuth 2.0 / OpenID Connect
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| OAuth 2.0 authorization server deployed | 🔴 CRITICAL | ☐ | IdentityServer, Auth0, Azure AD |
| OpenID Connect enabled | 🔴 CRITICAL | ☐ | ID tokens for user identity |
| Authorization code flow (not implicit) | 🔴 CRITICAL | ☐ | Implicit flow deprecated |
| PKCE required for all clients | 🔴 CRITICAL | ☐ | S256 challenge method |
| State parameter validated | 🔴 CRITICAL | ☐ | CSRF protection |
| Redirect URI strictly validated | 🔴 CRITICAL | ☐ | Exact match, no wildcards |

### 2.2 Multi-Factor Authentication
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| MFA required for clinicians | 🔴 CRITICAL | ☐ | TOTP, push notification, or hardware key |
| MFA required for admin access | 🔴 CRITICAL | ☐ | Hardware key preferred |
| Step-up authentication for sensitive operations | 🟠 HIGH | ☐ | Re-auth for bulk export, delete |
| MFA bypass documented and approved | 🟠 HIGH | ☐ | Break-glass procedures |

### 2.3 Session Management
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| Session timeout configured | 🔴 CRITICAL | ☐ | 15-30 minutes inactivity |
| Absolute session lifetime | 🔴 CRITICAL | ☐ | 8-12 hours maximum |
| Secure session cookies | 🔴 CRITICAL | ☐ | Secure, HttpOnly, SameSite=Strict |
| Session invalidation on logout | 🔴 CRITICAL | ☐ | Server-side session destroy |

---

## 3. Authorization

### 3.1 SMART on FHIR Scopes
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| SMART scopes implemented | 🔴 CRITICAL | ☐ | patient/*.read, user/*.read, etc. |
| Scope validation on every request | 🔴 CRITICAL | ☐ | Not just at token issuance |
| launch context validated | 🔴 CRITICAL | ☐ | Patient/encounter context |
| Resource-level scope enforcement | 🔴 CRITICAL | ☐ | patient/Observation.read vs patient/*.read |
| fhirUser claim validated | 🟠 HIGH | ☐ | Links token to FHIR resource |

### 3.2 Access Control
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| Patient compartment access validated | 🔴 CRITICAL | ☐ | Patients see only their data |
| Practitioner access logged | 🔴 CRITICAL | ☐ | Who accessed what patient |
| Admin access segregated | 🔴 CRITICAL | ☐ | Separate admin credentials |
| Service account scopes minimized | 🔴 CRITICAL | ☐ | Least privilege |
| Cross-patient access requires approval | 🟠 HIGH | ☐ | Population health, research |

---

## 4. Token Security

### 4.1 Token Configuration
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| Access token lifetime ≤ 60 minutes | 🔴 CRITICAL | ☐ | 15 minutes recommended |
| Refresh token rotation enabled | 🔴 CRITICAL | ☐ | New refresh token on use |
| Refresh token absolute lifetime | 🔴 CRITICAL | ☐ | 7-30 days maximum |
| Token revocation endpoint | 🔴 CRITICAL | ☐ | RFC 7009 compliance |
| Revoked tokens checked on every request | 🔴 CRITICAL | ☐ | Token blacklist/introspection |

### 4.2 DPoP / Sender Constraints (FAPI 2.0)
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| DPoP or mTLS implemented | 🟠 HIGH | ☐ | Required for FAPI 2.0 |
| DPoP proof validated (jti, iat, htu, htm) | 🟠 HIGH | ☐ | Replay protection |
| DPoP nonce supported | 🟠 HIGH | ☐ | Server-provided freshness |
| mTLS certificate validation | 🟠 HIGH | ☐ | Client certificate pinning |
| Key rotation documented | 🟠 HIGH | ☐ | 90-day rotation schedule |

---

## 5. Data Security

### 5.1 Encryption
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| Data encrypted at rest | 🔴 CRITICAL | ☐ | AES-256 minimum |
| Database encryption (TDE) | 🔴 CRITICAL | ☐ | Transparent data encryption |
| Backup encryption | 🔴 CRITICAL | ☐ | Same as production |
| Key management (HSM/KMS) | 🔴 CRITICAL | ☐ | No keys in code/config |
| Key rotation schedule | 🟠 HIGH | ☐ | Annual minimum |

### 5.2 Data Protection
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| FHIR security labels configured | 🟠 HIGH | ☐ | Restricted, confidential, etc. |
| _security parameter filtering | 🟠 HIGH | ☐ | Query-based label filtering |
| Data masking for non-production | 🔴 CRITICAL | ☐ | No real PHI in dev/test |
| Secure deletion procedure | 🟠 HIGH | ☐ | GDPR Article 17 |
| Data retention policy documented | 🟠 HIGH | ☐ | HIPAA 6-year retention |

---

## 6. Audit & Monitoring

### 6.1 Audit Logging
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| All API requests logged | 🔴 CRITICAL | ☐ | Who, what, when, where |
| Authentication events logged | 🔴 CRITICAL | ☐ | Success and failure |
| Authorization failures logged | 🔴 CRITICAL | ☐ | Scope violations |
| Admin actions logged | 🔴 CRITICAL | ☐ | Config changes, user management |
| Logs sent to SIEM | 🔴 CRITICAL | ☐ | Splunk, Sentinel, etc. |
| Log integrity protection | 🟠 HIGH | ☐ | Immutable storage |
| 6-year log retention | 🔴 CRITICAL | ☐ | HIPAA requirement |

### 6.2 Real-Time Monitoring
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| Failed authentication alerts | 🔴 CRITICAL | ☐ | >5 failures in 5 minutes |
| Unusual access pattern detection | 🟠 HIGH | ☐ | ML-based anomaly detection |
| Token abuse alerts | 🔴 CRITICAL | ☐ | Expired token usage, replay attempts |
| Rate limit alerts | 🟠 HIGH | ☐ | Near-limit warnings |
| On-call rotation configured | 🔴 CRITICAL | ☐ | 24/7 coverage |
| Incident response plan tested | 🔴 CRITICAL | ☐ | Tabletop exercise completed |

---

## 7. Application Security

### 7.1 API Security
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| Input validation (FHIR resources) | 🔴 CRITICAL | ☐ | Validate against profiles |
| SQL injection prevention | 🔴 CRITICAL | ☐ | Parameterized queries |
| NoSQL injection prevention | 🔴 CRITICAL | ☐ | Sanitize search parameters |
| XXE prevention | 🔴 CRITICAL | ☐ | Disable DTD processing |
| Response header security | 🟠 HIGH | ☐ | X-Content-Type, X-Frame-Options |
| Error handling (no stack traces) | 🔴 CRITICAL | ☐ | Generic error messages |
| Dependency scanning | 🔴 CRITICAL | ☐ | Snyk, Dependabot, etc. |

### 7.2 Code Security
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| SAST (static analysis) passed | 🟠 HIGH | ☐ | SonarQube, Fortify |
| DAST (dynamic analysis) passed | 🟠 HIGH | ☐ | OWASP ZAP, Burp Suite |
| Secrets scanning enabled | 🔴 CRITICAL | ☐ | No credentials in code |
| Code review completed | 🔴 CRITICAL | ☐ | Security-focused review |
| Penetration test completed | 🔴 CRITICAL | ☐ | Within last 12 months |

---

## 8. Compliance & Governance

### 8.1 HIPAA Security Rule
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| §164.308 Administrative safeguards | 🔴 CRITICAL | ☐ | Risk analysis, policies |
| §164.310 Physical safeguards | 🔴 CRITICAL | ☐ | Facility access, device security |
| §164.312 Technical safeguards | 🔴 CRITICAL | ☐ | Access control, encryption |
| Business Associate Agreement (BAA) | 🔴 CRITICAL | ☐ | All vendors covered |
| Breach notification procedure | 🔴 CRITICAL | ☐ | 60-day notification |

### 8.2 GDPR (if applicable)
| Item | Severity | Status | Notes |
|------|----------|--------|-------|
| Article 32 security measures | 🔴 CRITICAL | ☐ | Appropriate technical measures |
| Data Processing Agreement (DPA) | 🔴 CRITICAL | ☐ | All processors covered |
| Right to erasure implemented | 🟠 HIGH | ☐ | Article 17 |
| Data portability (FHIR export) | 🟠 HIGH | ☐ | Article 20 |
| Consent management | 🔴 CRITICAL | ☐ | Documented consent |

---

## Sign-Off

### Pre-Deployment Approval

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Security Lead | | | |
| Development Lead | | | |
| Compliance Officer | | | |
| CISO | | | |
| Project Sponsor | | | |

### Go/No-Go Decision

- [ ] All CRITICAL items completed
- [ ] All HIGH items completed or documented risk accepted
- [ ] Penetration test within last 12 months
- [ ] Incident response plan tested
- [ ] On-call rotation configured
- [ ] Rollback plan documented

**Decision:** ☐ GO / ☐ NO-GO

**Authorized By:** _________________________ **Date:** _____________

---

## Post-Deployment Validation

| Item | Status | Date Completed | Notes |
|------|--------|----------------|-------|
| Production smoke test | ☐ | | |
| Monitoring alerts working | ☐ | | |
| Backup verified | ☐ | | |
| Rollback tested | ☐ | | |
| 24-hour monitoring period passed | ☐ | | |

---

*Document Version Control: Track all changes in your organization's document management system.*
