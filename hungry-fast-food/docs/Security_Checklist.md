🔒 Security Checklist (NON-NEGOTIABLE)
#	Security Measure	Implementation
1	JWT Authentication	Access token (15min) + Refresh token (7days)
2	Password Hashing	bcrypt (salt rounds = 12)
3	Rate Limiting	100 requests per IP per 15min
4	SQL Injection Protection	Parameterized queries only
5	XSS Protection	DOMPurify + CSP headers
6	CSRF Protection	CSRF tokens on all forms
7	HTTPS Only	Enforce SSL/TLS
8	Environment Variables	All secrets in .env
9	Input Validation	Joi/Zod validation all inputs
10	Suspicious Order Detection	Admin needs credentials to cancel delivered orders
11	Audit Logs	All admin actions logged
12	CORS Restriction	Only allow your domain
13	OTP Expiry	10 minutes with 3 attempts max
14	Brute Force Protection	Lock after 5 failed login attempts
15	API Key for Admin Panel	Secure communication with backend