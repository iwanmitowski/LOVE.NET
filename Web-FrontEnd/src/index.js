import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import { BrowserRouter } from 'react-router-dom';
import { IdentityProvider } from './contexts/IdentityContext';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <IdentityProvider>
    <BrowserRouter>
      <App />
    </BrowserRouter>  
  </IdentityProvider>
);

reportWebVitals();
