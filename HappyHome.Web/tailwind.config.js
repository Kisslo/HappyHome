/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{js,jsx,ts,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        happy: {
          DEFAULT: '#4CAF8A',
          dark:    '#3a8a6c',
          light:   '#e6f4ee',
          ink:     '#1f2d28',
        },
      },
    },
  },
  plugins: [],
};
