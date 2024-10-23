/** @type {import('tailwindcss').Config} */
module.exports = {
	content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
	theme: {
		extend: {
			screens: {
				tablet: "640px",
				// => @media (min-width: 640px) { ... }

				laptop: "1024px",
				// => @media (min-width: 1024px) { ... }

				desktop: "1280px",
				// => @media (min-width: 1280px) { ... }
			},
			boxShadow: {
				top: "0px 5px 10px 0px rgba(0, 0, 0, 0.25)", // #00000040 is rgba(0, 0, 0, 0.25)
			},
			colors: {
				//primevue
				"primary-50": "rgb(var(--primary-50))",
				"primary-100": "rgb(var(--primary-100))",
				"primary-200": "rgb(var(--primary-200))",
				"primary-300": "rgb(var(--primary-300))",
				"primary-400": "rgb(var(--primary-400))",
				"primary-500": "rgb(var(--primary-500))",
				"primary-600": "rgb(var(--primary-600))",
				"primary-700": "rgb(var(--primary-700))",
				"primary-800": "rgb(var(--primary-800))",
				"primary-900": "rgb(var(--primary-900))",
				"primary-950": "rgb(var(--primary-950))",
				"surface-0": "rgb(var(--surface-0))",
				"surface-50": "rgb(var(--surface-50))",
				"surface-100": "rgb(var(--surface-100))",
				"surface-200": "rgb(var(--surface-200))",
				"surface-300": "rgb(var(--surface-300))",
				"surface-400": "rgb(var(--surface-400))",
				"surface-500": "rgb(var(--surface-500))",
				"surface-600": "rgb(var(--surface-600))",
				"surface-700": "rgb(var(--surface-700))",
				"surface-800": "rgb(var(--surface-800))",
				"surface-900": "rgb(var(--surface-900))",
				"surface-950": "rgb(var(--surface-950))",

				"dark": "#000000",
				"light-purple": "#E8DEF8",
				"light-pink": "#A55678",
				"dark-pink": " #80455E",
				"deep-blue": "#19234B",
				"navy-blue": "#061830",
				"purple": "#EFB2CE",
				"deep-dark": "#00000080",
				"dark-purple": "#3E1F3C",
				"pink-purple":"#63284E",

			},
			borderRadius: {
				4: "4px",
				"1/2": "50%",
			},
			width: {
				"1/4": "25%",
				"1/2": "50%",
				"3/4": "75%",
			},
			minWidth: {
				"1/4": "25%",
				"1/2": "50%",
				"3/4": "75%",
			},
			maxWidth: {
				"1/4": "25%",
				"1/2": "50%",
				"3/4": "75%",
			},
			minHeight: {
				"1/4": "25%",
				"1/2": "50%",
				"3/4": "75%",
			},
			maxHeight: {
				"1/4": "25%",
				"1/2": "50%",
				"3/4": "75%",
			},
			zIndex: {
				100: "100", // Full-page Overlay
				200: "200", // Settings Modal
				1000: "1000", // Button Menu
				10000: "10000", // Overlay Window
			},
			gridTemplateRows: {
				7: "repeat(7, minmax(0, 1fr))",
			},
			backgroundImage: {
				"gradient-to-b":
					"linear-gradient(to bottom, #593359 38%, #19234B 79%, #061830 100%)",
				"gradient-to-t":
					"linear-gradient(180deg, #253162 0%, #141D3F 37.5%, #0D1225 100%)",
				"hero-gradient":
					"linear-gradient(180deg, #72355A 0%, #432D54 19.67%, #2F2850 45.17%, #19234B 74.17%, #061830 95%)",
				"signin-gradient":
					"linear-gradient(to bottom, #A75779 0%, #72395E 18.5%, #19234B 66.5%, #061830 95%)",
				"profile-gradient":
					"linear-gradient(to bottom, #432D54 0%, #2F2850 31.5%, #19234B 60%, #061830 95%)",
			},
			fontSize: {
				"custom-lg": "10.375rem", // Equivalent to approximately 166px (for large screens)
				"custom-md": "6.25rem", // Equivalent to approximately 100px (for medium screens)
				"custom-sm": "5rem", // Equivalent to approximately 80px (for small screens)
				"custom-xs": "4rem", // Equivalent to approximately 64px (for extra small screens)
			},
		},
	},
	plugins: [],
};