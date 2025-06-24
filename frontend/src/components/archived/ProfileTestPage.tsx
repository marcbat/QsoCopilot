import React, { useState } from 'react';

// Composant de test sans aucun contexte
const ProfileTestPage: React.FC = () => {
  const [message, setMessage] = useState<string | null>(null);
  
  console.log('ProfileTestPage rendu - message:', message);

  const handleTest = () => {
    console.log('Test button clicked');
    setMessage('Test message de succès !');
    
    // Manipulation DOM directe aussi
    const messageDiv = document.getElementById('test-message-container');
    if (messageDiv) {
      messageDiv.innerHTML = `<div style="background: green; color: white; padding: 10px; border-radius: 5px;">Message DOM: Test de succès !</div>`;
      messageDiv.style.display = 'block';
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1>Page de Test (sans contexte)</h1>
        
        <button 
          onClick={handleTest}
          className="btn btn-primary"
          style={{ marginBottom: '1rem' }}
        >
          Tester l'affichage de message
        </button>

        {/* Message avec état React */}
        {message && <div className="success-message">{message}</div>}
        
        {/* Message avec manipulation DOM */}
        <div id="test-message-container" style={{display: 'none', marginBottom: '1rem'}}></div>
        
        {/* Debug */}
        <div style={{border: '1px solid blue', padding: '5px', margin: '5px'}}>
          Debug - message state: "{message}"
        </div>
        
        <p>Cette page ne utilise aucun contexte d'authentification.</p>
        <p>Si les re-rendus en boucle s'arrêtent ici, le problème vient de useAuth().</p>
      </div>
    </div>
  );
};

export default ProfileTestPage;
