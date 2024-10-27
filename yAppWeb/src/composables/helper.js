export const getCurrentTime = ()=> {
	const now = new Date();

	// Get ISO string, which is in the form of "YYYY-MM-DDTHH:mm:ss.sssZ"
	let isoString = now.toISOString(); // Example: "2024-10-26T19:00:23.123Z"

	// Add fractional seconds (example: .5)
	const milliseconds = now.getMilliseconds();
	const fractionalSeconds = Math.floor(milliseconds / 100); // Round to nearest tenth
	isoString = isoString.replace(/\.\d{3}Z$/, `.${fractionalSeconds}Z`);

	// Handle timezone offset
	const timezoneOffset = now.getTimezoneOffset();
	const offsetSign = timezoneOffset <= 0 ? "+" : "-";
	const offsetHours = String(
		Math.abs(Math.floor(timezoneOffset / 60)),
	).padStart(2, "0");
	const offsetMinutes = String(Math.abs(timezoneOffset % 60)).padStart(2, "0");

	const formattedOffset = `${offsetSign}${offsetHours}:${offsetMinutes}`;

	// Replace "Z" with the correct timezone offset
	isoString = isoString.replace("Z", formattedOffset);

	return isoString;
}

