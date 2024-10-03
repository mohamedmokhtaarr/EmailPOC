using EmailPOC.Interfaces;
using EmailPOC.Settings;
using System.Text.RegularExpressions;

namespace EmailPOC.Helpers
{
	class ValidationHelper : IValidationHelper
	{
		private ValidationHelperSettings ValidationHelperSettings { get; }
		private readonly Regex _mailRegex;
		private readonly Regex _digitsOnlyRegex;
		private readonly Regex _egyptianMobileNumberRegex;
		private readonly Regex _longitudeLatitudeValidationRegex;
		private readonly Regex _placeholdersRegex;
		private readonly Regex _lettersOnly;

		public ValidationHelper(ValidationHelperSettings validationHelperSettings)
		{
			ValidationHelperSettings = validationHelperSettings;
			_mailRegex = new Regex(ValidationHelperSettings.MailValidationRegex, RegexOptions.IgnoreCase);
			_digitsOnlyRegex = new Regex(validationHelperSettings.DigitsOnlyRegex, RegexOptions.Compiled);
			_egyptianMobileNumberRegex = new Regex(validationHelperSettings.EgyptianMobileNumberRegex, RegexOptions.Compiled);
			_longitudeLatitudeValidationRegex = new Regex(validationHelperSettings.LongitudeLatitudeValidationRegex, RegexOptions.Compiled);
			_placeholdersRegex = new Regex(validationHelperSettings.PlaceholdersRegex, RegexOptions.Compiled);
			_lettersOnly = new Regex(validationHelperSettings.LettersOnly, RegexOptions.Compiled);

		}

		public Dictionary<string, bool> ValidateEmailList(List<string> mailList)
		{
			Dictionary<string, bool> mailResult = new Dictionary<string, bool>();

			mailList?.ForEach(m =>
			{
				if (!mailResult.ContainsKey(m)) mailResult.Add(m, ValidateMail(m));
			});

			return mailResult;
		}
		public bool ValidateMail(string mail) => _mailRegex.IsMatch(mail);

		public bool ValidateEgyptianMobileNumber(string mobileNumber) => _egyptianMobileNumberRegex.IsMatch(mobileNumber);
		public bool ValidateLettersOnly(string value) => _lettersOnly.IsMatch(value);
		public bool ValidateDigitsOnly(string value) => _digitsOnlyRegex.IsMatch(value);

		public bool ValidateLongitudeLatitude(decimal longitude) => _longitudeLatitudeValidationRegex.IsMatch(longitude.ToString());
		public bool HasPlaceholders(string input) => _placeholdersRegex.IsMatch(input);

	}
}
