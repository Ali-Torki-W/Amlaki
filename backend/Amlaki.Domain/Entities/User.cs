namespace Domain.Entities.User;

using Domain.Primitives;
using Domain.Exceptions;
using Domain.Enums.User;
using Domain.ValueObjects.User;

public sealed class User : AggregateRoot<Guid>
{
    // Identity & contact
    public Email? Email { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public bool IsPhoneVerified { get; private set; }

    // Auth
    public PasswordHash? Password { get; private set; }  // null means passwordless account (e.g., OTP-only)

    // Profile & KYC
    public PersonName? Name { get; private set; }
    public KycProfile? Kyc { get; private set; }
    public KycStatus KycStatus { get; private set; }

    // Roles & status
    public UserRole Roles { get; private set; }
    public UserStatus Status { get; private set; }

    // Audit
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private User() : base(Guid.Empty) { } // ORM

    private User(Email? email, PhoneNumber? phone, PasswordHash? password, PersonName? name) : base(Guid.NewGuid())
    {
        if (email is null && phone is null)
            throw new DomainException("At least one of Email or Phone is required.");

        Email = email;
        Phone = phone;
        Password = password;
        Name = name;

        IsEmailVerified = false;
        IsPhoneVerified = false;

        Roles = UserRole.User;      // default role
        Status = UserStatus.Active;
        KycStatus = KycStatus.NotSubmitted;

        CreatedAtUtc = UpdatedAtUtc = DateTime.UtcNow;
    }

    // Factory: Register new user
    public static User RegisterWithEmail(string email, PasswordHash? password, PersonName? name = null)
        => new(User.EmailFrom(email), null, password, name);

    public static User RegisterWithPhone(string phone, PasswordHash? password, PersonName? name = null)
        => new(null, PhoneNumber.Create(phone), password, name);

    public static User RegisterWithEmailAndPhone(string email, string phone, PasswordHash? password, PersonName? name = null)
        => new(User.EmailFrom(email), PhoneNumber.Create(phone), password, name);

    private static Email EmailFrom(string e) => Email.Create(e);

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    // ===== Contact & Verification rules =====

    public void ChangeEmail(Email newEmail)
    {
        if (Status != UserStatus.Active) throw new DomainException("Only active users can change email.");
        if (Email is not null && newEmail.Value == Email.Value) return;

        Email = newEmail;
        IsEmailVerified = false; // MUST re-verify on change
        Touch();
    }

    public void VerifyEmail()
    {
        if (Email is null) throw new DomainException("No email to verify.");
        IsEmailVerified = true;
        Touch();
    }

    public void ChangePhone(PhoneNumber newPhone)
    {
        if (Status != UserStatus.Active) throw new DomainException("Only active users can change phone.");
        if (Phone is not null && newPhone.E164 == Phone.E164) return;

        Phone = newPhone;
        IsPhoneVerified = false; // MUST re-verify on change
        Touch();
    }

    public void VerifyPhone()
    {
        if (Phone is null) throw new DomainException("No phone to verify.");
        IsPhoneVerified = true;
        Touch();
    }

    // ===== Authentication rules =====

    public void SetPassword(PasswordHash newHash)
    {
        if (Status == UserStatus.Deactivated) throw new DomainException("Deactivated users cannot set password.");
        Password = newHash;
        Touch();
    }

    public void RemovePassword() // e.g., switch to OTP-only login
    {
        if (Status != UserStatus.Active) throw new DomainException("Only active users can remove password.");
        Password = null;
        Touch();
    }

    // ===== Profile & KYC =====

    public void SetName(PersonName name)
    {
        if (Status != UserStatus.Active) throw new DomainException("Only active users can change name.");
        Name = name;
        Touch();
    }

    public void SubmitKyc(KycProfile profile)
    {
        if (Status != UserStatus.Active) throw new DomainException("Only active users can submit KYC.");
        Kyc = profile;
        KycStatus = KycStatus.Pending;
        Touch();
    }

    public void ApproveKyc()
    {
        if (Kyc is null) throw new DomainException("KYC profile must be submitted first.");
        KycStatus = KycStatus.Approved;
        Touch();
    }

    public void RejectKyc()
    {
        if (Kyc is null) throw new DomainException("KYC profile must be submitted first.");
        KycStatus = KycStatus.Rejected;
        Touch();
    }

    // ===== Roles & Elevation rules =====

    public void GrantRole(UserRole role)
    {
        if (Status != UserStatus.Active) throw new DomainException("Only active users can receive roles.");
        if ((Roles & role) != 0) return; // already has role

        // Business rule examples:
        // - Agent role requires email & phone verified + KYC approved.
        if (role.HasFlag(UserRole.Agent))
        {
            if (!IsEmailVerified || !IsPhoneVerified) throw new DomainException("Agent requires verified email and phone.");
            if (KycStatus != KycStatus.Approved) throw new DomainException("Agent requires approved KYC.");
        }

        Roles |= role;
        Touch();
    }

    public void RevokeRole(UserRole role)
    {
        if (role == UserRole.Admin) throw new DomainException("Admin role cannot be revoked here.");
        Roles &= ~role;
        Touch();
    }

    // ===== Account status rules =====

    public void Suspend()
    {
        if (Status == UserStatus.Deactivated) throw new DomainException("Cannot suspend a deactivated user.");
        Status = UserStatus.Suspended;
        Touch();
    }

    public void Reactivate()
    {
        if (Status == UserStatus.Deactivated) throw new DomainException("Cannot reactivate a deactivated user.");
        Status = UserStatus.Active;
        Touch();
    }

    public void Deactivate()
    {
        Status = UserStatus.Deactivated;
        // Optional: strip non-essential roles on deactivation
        Touch();
    }
}
