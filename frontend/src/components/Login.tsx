import React, { useState } from 'react';
import './Login.css';

interface LoginProps {
  onLogin: (username: string) => void;
}

const Login: React.FC<LoginProps> = ({ onLogin }) => {
  const [username, setUsername] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!username.trim()) {
      setError('Please enter a username');
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const response = await fetch('http://localhost:3001/api/player/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ username: username.trim() }),
      });

      if (!response.ok) {
        throw new Error(`Login failed: ${response.statusText}`);
      }

      const data = await response.json();
      console.log('Login successful:', data);
      
      // Call parent callback without storing in localStorage
      onLogin(data.username);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred during login');
      console.error('Login error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-box">
        <h1>Villagers</h1>
        <h2>Enter Your Village</h2>
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <input
              type="text"
              placeholder="Enter your username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={isLoading}
              className="username-input"
              maxLength={20}
              autoFocus
            />
          </div>
          
          {error && <div className="error-message">{error}</div>}
          
          <button 
            type="submit" 
            disabled={isLoading || !username.trim()}
            className="login-button"
          >
            {isLoading ? 'Entering...' : 'Enter World'}
          </button>
        </form>
        
        <p className="login-info">
          No account needed - just enter a username to start playing!
        </p>
      </div>
    </div>
  );
};

export default Login;