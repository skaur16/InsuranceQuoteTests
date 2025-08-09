using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace InsuranceQuoteTests
{
    [TestFixture]
    public class InsuranceQuoteTests : IDisposable
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private bool disposed = false;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArguments("--start-maximized", "--disable-notifications");
            driver = new ChromeDriver(options);

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            driver.Navigate().GoToUrl("http://localhost/prog8170a04/getQuote.html");
            wait.Until(d => d.FindElement(By.Id("btnSubmit")).Displayed);
        }

        private void FillPersonalInfo(bool valid = true)
        {
            // Arrange: Fill personal information
            driver.FindElement(By.Id("firstName")).SendKeys("Sharan");
            driver.FindElement(By.Id("lastName")).SendKeys("Kaur");
            driver.FindElement(By.Id("address")).SendKeys("123 Main St");
            driver.FindElement(By.Id("city")).SendKeys("Waterloo");

            if (valid)
            {
                driver.FindElement(By.Id("postalCode")).SendKeys("N2L 3G1");  
                driver.FindElement(By.Id("phone")).SendKeys("519-555-1234");
                driver.FindElement(By.Id("email")).SendKeys("skaur@gmail.com");  
            }
        }

        private void FillDrivingInfo(string age, string experience, string accidents)
        {
            // Arrange: Fill driving information
            driver.FindElement(By.Id("age")).Clear();
            if (!string.IsNullOrEmpty(age)) driver.FindElement(By.Id("age")).SendKeys(age);

            driver.FindElement(By.Id("experience")).Clear();
            if (!string.IsNullOrEmpty(experience)) driver.FindElement(By.Id("experience")).SendKeys(experience);

            driver.FindElement(By.Id("accidents")).Clear();
            if (!string.IsNullOrEmpty(accidents)) driver.FindElement(By.Id("accidents")).SendKeys(accidents);
        }

        private void SubmitForm()
        {
            // Act: Submit the form
            driver.FindElement(By.Id("btnSubmit")).Click();
        }

        private string GetQuoteResult()
        {
            // Return the quote result after waiting
            return wait.Until(d => d.FindElement(By.Id("finalQuote")).GetAttribute("value"));
        }

        private string GetValidationMessage(string fieldId)
        {
            try
            {
                var field = driver.FindElement(By.Id(fieldId));
                var html5Message = field.GetAttribute("validationMessage");
                if (!string.IsNullOrEmpty(html5Message)) return html5Message;

                var classAttribute = field.GetAttribute("class");
                if (classAttribute.Contains("error") || classAttribute.Contains("invalid"))
                    return "Invalid field";

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Test 1: Valid Data 
        [Test]
        public void InsuranceQuote01_ValidData_Quote5500()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("24", "3", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(), Is.EqualTo("$5500"));
        }

        // Test 2: 4 Accidents 
        [Test]
        public void InsuranceQuote02_InsuranceDenied_4Accidents()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("25", "3", "4");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(),
                Is.EqualTo("No Insurance for you!!  Too many accidents - go take a course!"));
        }

        // Test 3: Valid With Discount 
        [Test]
        public void InsuranceQuote03_ValidWithDiscount_Quote3905()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("35", "9", "2");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(), Is.EqualTo("$3905"));
        }


        // Test 4: Invalid Phone Number 
        [Test]
        public void InsuranceQuote04_InvalidPhoneNumber_Error()
        {
            // Arrange
            FillPersonalInfo(false);
            driver.FindElement(By.Id("postalCode")).SendKeys("N2L 3G1");
            driver.FindElement(By.Id("phone")).SendKeys("123");  
            driver.FindElement(By.Id("email")).SendKeys("skaur@gmail.com");
            FillDrivingInfo("27", "3", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetValidationMessage("phone"), Is.Not.Empty);
        }

        // Test 5: Invalid Email = Error
        [Test]
        public void InsuranceQuote05_InvalidEmail_Error()
        {
            // Arrange
            FillPersonalInfo(false);
            driver.FindElement(By.Id("postalCode")).SendKeys("N2L 3G1");
            driver.FindElement(By.Id("phone")).SendKeys("519-555-1234");
            driver.FindElement(By.Id("email")).SendKeys("Skaur@gmail.com");  
            FillDrivingInfo("28", "3", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetValidationMessage("email"), Is.Not.Empty);
        }

        // Test 6: Invalid Postal Code 
        [Test]
        public void InsuranceQuote06_InvalidPostalCode_Error()
        {
            // Arrange
            FillPersonalInfo(false);
            driver.FindElement(By.Id("postalCode")).SendKeys("12345");
            driver.FindElement(By.Id("phone")).SendKeys("519-555-1234");
            driver.FindElement(By.Id("email")).SendKeys("Skaur@gmail.com");
            FillDrivingInfo("35", "15", "1");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetValidationMessage("postalCode"), Is.Not.Empty);
        }

        // Test 7: Age Omitted = Error
        [Test]
        public void InsuranceQuote07_AgeOmitted_Error()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("", "5", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetValidationMessage("age"), Is.Not.Empty);
        }

        // Test 8: Accidents Omitted = Error
        [Test]
        public void InsuranceQuote08_AccidentsOmitted_Error()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("37", "8", "");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetValidationMessage("accidents"), Is.Not.Empty);
        }

        // Test 9: Experience
        [Test]
        public void InsuranceQuote09_ExperienceOmitted_Error()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("45", "", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetValidationMessage("experience"), Is.Not.Empty);
        }

        // Test 10: Minimum Age (16) 
        [Test]
        public void InsuranceQuote10_MinimumAge_Quote7000()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("16", "0", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(), Is.EqualTo("$7000"));
        }

        // Test 11: Age 30 with 2 Years Exp = $3905 Quote
        [Test]
        public void InsuranceQuote11_Age30_2YearsExp_Quote3905()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("30", "2", "1");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(), Is.EqualTo("$3905"));
        }

        // Test 12: Max Experience Difference = $2840 Quote
        [Test]
        public void InsuranceQuote12_MaxExperienceDiff_Quote2840()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("45", "29", "1");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(), Is.EqualTo("$2840"));
        }

        // Test 13: Invalid Age (15) = Error
        [Test]
        public void InsuranceQuote13_InvalidAge15_Error()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("15", "0", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetValidationMessage("age"), Is.Not.Empty);
        }

        // Test 14: Invalid Experience = Error
        [Test]
        public void InsuranceQuote14_InvalidExperience_Error()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("20", "5", "0");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(),
                Is.EqualTo("No Insurance for you!! Driver Age / Experience Not Correct"));
        }

        // Test 15: Valid Data = $2840 Quote
        [Test]
        public void InsuranceQuote15_ValidData_Quote2840()
        {
            // Arrange
            FillPersonalInfo();
            FillDrivingInfo("40", "10", "2");

            // Act
            SubmitForm();

            // Assert
            Assert.That(GetQuoteResult(), Is.EqualTo("$2840"));
        }

        [TearDown]
        public void Teardown()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                try
                {
                    driver?.Quit();
                    driver?.Dispose();
                }
                catch { }
                disposed = true;
            }
        }
    }
}