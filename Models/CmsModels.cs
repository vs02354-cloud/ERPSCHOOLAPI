using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class HomePageSettings
    {
        [Key]
        public int Id { get; set; }
        
        // Branding
        public string SchoolName { get; set; } = "EduSchool";
        public string LogoUrl { get; set; } = "assets/logo.jpeg";
        public string Address { get; set; } = "Super Corridor, Indore, Madhya Pradesh";
        public string Email { get; set; } = "info@eduschool.edu";
        public string Phone { get; set; } = "+1 (555) 123-4567";
        public string WebsiteUrl { get; set; } = "https://www.eduschool.edu";
        
        // Hero Section
        public string HeroTagline { get; set; } = "Welcome to Excellence";
        public string HeroHeading { get; set; } = "Empowering Minds, Shaping the Future.";
        public string HeroDescription { get; set; } = "A premier educational institution dedicated to fostering academic excellence, innovation, and character development in every student.";
        public string HeroPrimaryButtonText { get; set; } = "Admission Inquiry";
        public string HeroPrimaryButtonUrl { get; set; } = "/inquiry";
        public string HeroSecondaryButtonText { get; set; } = "Student / Parent Portal";
        public string HeroSecondaryButtonUrl { get; set; } = "/login";

        // Location Map
        public string MapEmbedUrl { get; set; } = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3679.529517170564!2d75.82025687590827!3d22.745700779368367!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3962fd02e2124cb1%3A0x67db238d212133b3!2sSuper%20Corridor%2C%20Indore%2C%20Madhya%20Pradesh!5e0!3m2!1sen!2sin!4v1700000000000!5m2!1sen!2sin";
    }

    public class QuickLink
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Url { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SocialMediaLink
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Platform { get; set; } // Facebook, Instagram, Twitter, etc.
        [Required]
        public string Url { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpcomingEvent
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class RecentActivity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FacultyExcellence
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Achievement { get; set; }
        public string PhotoUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class StudentSpotlight
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Class { get; set; }
        public string Achievement { get; set; }
        public string PhotoUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class HomeStatistic
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Label { get; set; }
        [Required]
        public string Value { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class NewsTicker
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string NoticeText { get; set; }
        public string NoticeType { get; set; } // NEW, URGENT, INFO
        public int Priority { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PortalCard
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconSvg { get; set; } // SVG string or Icon Name
        public string Url { get; set; }
        public string ThemeColor { get; set; } // e.g. blue, purple, green
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ImageGallery
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CmsAuditLog
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; } // Created, Updated, Deleted
        public string Module { get; set; } // HeroSection, QuickLinks, etc.
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string PreviousValue { get; set; } // JSON serialized
        public string NewValue { get; set; } // JSON serialized
    }

    public class Holiday
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Type { get; set; } // Public Holiday, Government Holiday, Optional Holiday
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
