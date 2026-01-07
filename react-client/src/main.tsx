import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';

// Simple CSS reset
const style = document.createElement('style');
style.textContent = `
  * {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
  }
  body {
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    line-height: 1.5;
    color: #333;
    background-color: #f5f5f5;
  }
`;
document.head.appendChild(style);

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root')
);
