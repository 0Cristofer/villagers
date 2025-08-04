import React, { useState, useEffect } from 'react';
import './App.css';
import { urls } from './config/env';
import { Player, AuthResponse, LoginRequest, RegisterRequest, WorldResponse } from './types/api';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);
  const [player, setPlayer] = useState<Player | null>(null);
  const [, setToken] = useState<string | null>(null);
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [isRegisterMode, setIsRegisterMode] = useState<boolean>(false);
  const [availableWorlds, setAvailableWorlds] = useState<WorldResponse[]>([]);
  const [worldsLoading, setWorldsLoading] = useState<boolean>(false);

  // Check for existing token on component mount
  useEffect(() => {
    const storedToken = localStorage.getItem('villagers_token');
    if (storedToken) {
      validateExistingToken(storedToken);
    }
  }, []);

  const validateExistingToken = async (storedToken: string) => {
    try {
      setIsLoading(true);
      const response = await fetch(urls.auth.validate, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${storedToken}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const authData: AuthResponse = await response.json();
        setToken(authData.token);
        setPlayer(authData.player);
        setIsLoggedIn(true);
        localStorage.setItem('villagers_token', authData.token);
        
        // Fetch available worlds after successful token validation
        await fetchAvailableWorlds();
      } else {
        // Token is invalid, remove it
        localStorage.removeItem('villagers_token');
      }
    } catch (error) {
      console.error('Token validation failed:', error);
      localStorage.removeItem('villagers_token');
    } finally {
      setIsLoading(false);
    }
  };

  const fetchAvailableWorlds = async () => {
    try {
      setWorldsLoading(true);
      const response = await fetch(`${urls.api.base}/api/worlds`);
      if (response.ok) {
        const worlds: WorldResponse[] = await response.json();
        setAvailableWorlds(worlds);
      } else {
        console.error('Failed to fetch worlds');
      }
    } catch (error) {
      console.error('Error fetching worlds:', error);
    } finally {
      setWorldsLoading(false);
    }
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!username.trim() || !password.trim()) return;

    setIsLoading(true);
    setError('');

    try {
      const loginRequest: LoginRequest = { 
        username: username.trim(),
        password: password.trim()
      };
      
      const response = await fetch(urls.auth.login, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(loginRequest)
      });

      if (response.ok) {
        const authData: AuthResponse = await response.json();
        setToken(authData.token);
        setPlayer(authData.player);
        setIsLoggedIn(true);
        
        // Store token for future sessions
        localStorage.setItem('villagers_token', authData.token);
        
        // Fetch available worlds after successful login
        await fetchAvailableWorlds();
      } else {
        // Handle different response types
        const errorText = await response.text();
        try {
          const errorData = JSON.parse(errorText);
          setError(errorData.message || 'Login failed');
        } catch {
          // If it's not JSON, use the text directly (for Unauthorized responses)
          setError(errorText || 'Login failed');
        }
      }
    } catch (error) {
      console.error('Login error:', error);
      setError('Failed to connect to server');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!username.trim() || !password.trim()) return;

    setIsLoading(true);
    setError('');

    try {
      const registerRequest: RegisterRequest = { 
        username: username.trim(),
        password: password.trim()
      };
      
      const response = await fetch(urls.auth.register, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(registerRequest)
      });

      if (response.ok) {
        const authData: AuthResponse = await response.json();
        setToken(authData.token);
        setPlayer(authData.player);
        setIsLoggedIn(true);
        
        // Store token for future sessions
        localStorage.setItem('villagers_token', authData.token);
        
        // Fetch available worlds after successful registration
        await fetchAvailableWorlds();
      } else {
        // Handle different response types
        const errorText = await response.text();
        try {
          const errorData = JSON.parse(errorText);
          setError(errorData.message || 'Registration failed');
        } catch {
          // If it's not JSON, use the text directly
          setError(errorText || 'Registration failed');
        }
      }
    } catch (error) {
      console.error('Registration error:', error);
      setError('Failed to connect to server');
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setPlayer(null);
    setToken(null);
    setUsername('');
    setPassword('');
    setIsRegisterMode(false);
    localStorage.removeItem('villagers_token');
  };

  const toggleMode = () => {
    setIsRegisterMode(!isRegisterMode);
    setError('');
    setUsername('');
    setPassword('');
  };

  const renderAuthForm = () => (
    <div className="login-container">
      <div className="login-form">
        <h2>Welcome to Villagers</h2>
        <p>{isRegisterMode ? 'Create a new account' : 'Enter your credentials to continue'}</p>
        
        <form onSubmit={isRegisterMode ? handleRegister : handleLogin}>
          <div className="form-group">
            <label htmlFor="username">Username:</label>
            <input
              id="username"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Enter username"
              disabled={isLoading}
              autoFocus
              minLength={3}
              maxLength={50}
            />
            {isRegisterMode && (
              <small className="form-hint">Username must be 3-50 characters</small>
            )}
          </div>
          
          <div className="form-group">
            <label htmlFor="password">Password:</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter password"
              disabled={isLoading}
              minLength={1}
              maxLength={100}
            />
            {isRegisterMode && (
              <small className="form-hint">Password must be at least 1 character</small>
            )}
          </div>
          
          {error && <div className="error-message">{error}</div>}
          
          <button 
            type="submit" 
            disabled={isLoading || !username.trim() || !password.trim()}
            className="login-button"
          >
            {isLoading 
              ? (isRegisterMode ? 'Creating Account...' : 'Logging in...') 
              : (isRegisterMode ? 'Create Account' : 'Login')
            }
          </button>
        </form>
        
        <div className="auth-toggle">
          <p>
            {isRegisterMode ? 'Already have an account?' : "Don't have an account?"}{' '}
            <button 
              type="button" 
              onClick={toggleMode}
              className="toggle-button"
              disabled={isLoading}
            >
              {isRegisterMode ? 'Login' : 'Register'}
            </button>
          </p>
        </div>
      </div>
    </div>
  );

  const renderWorldList = () => {
    const registeredWorlds = availableWorlds.filter(world => 
      player?.registeredWorldIds.includes(world.worldId)
    );
    const unregisteredWorlds = availableWorlds.filter(world => 
      !player?.registeredWorldIds.includes(world.worldId)
    );

    return (
      <div className="world-list-container">
        <div className="header">
          <h1>Villagers Game</h1>
          <div className="user-info">
            <span>Welcome, {player?.username}!</span>
            <button onClick={handleLogout} className="logout-button">
              Logout
            </button>
          </div>
        </div>

        <div className="worlds-container">
          <div className="registered-worlds">
            <h3>Your Worlds</h3>
            {worldsLoading ? (
              <p>Loading worlds...</p>
            ) : registeredWorlds.length > 0 ? (
              <div className="worlds-grid">
                {registeredWorlds.map(world => (
                  <div key={world.worldId} className="world-card registered">
                    <h4>{world.config.worldName}</h4>
                    <p>Server: {world.serverEndpoint}</p>
                    <p>Tick Interval: {world.config.tickInterval}</p>
                    <p>Registered: {new Date(world.registeredAt).toLocaleDateString()}</p>
                    <button className="enter-world-button">Join World</button>
                  </div>
                ))}
              </div>
            ) : (
              <p>You haven't joined any worlds yet.</p>
            )}
          </div>

          <div className="available-worlds">
            <h3>Available Worlds</h3>
            {worldsLoading ? (
              <p>Loading worlds...</p>
            ) : unregisteredWorlds.length > 0 ? (
              <div className="worlds-grid">
                {unregisteredWorlds.map(world => (
                  <div key={world.worldId} className="world-card available">
                    <h4>{world.config.worldName}</h4>
                    <p>Server: {world.serverEndpoint}</p>
                    <p>Tick Interval: {world.config.tickInterval}</p>
                    <p>Registered: {new Date(world.registeredAt).toLocaleDateString()}</p>
                    <button className="register-world-button" disabled>
                      Register (Coming Soon)
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <p>No worlds available at the moment.</p>
            )}
          </div>
        </div>
      </div>
    );
  };

  // Show loading spinner during initial token validation
  if (isLoading && !isLoggedIn && !username) {
    return (
      <div className="App">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Checking authentication...</p>
        </div>
      </div>
    );
  }


  return (
    <div className="App">
      {isLoggedIn && player ? renderWorldList() : renderAuthForm()}
    </div>
  );
}

export default App;