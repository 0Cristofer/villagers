import { useState, useEffect } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { TypedHubConnection, GameState, Village, GameClientMethods } from './types/signalr';
import { BuildBuildingRequest, RecruitTroopsRequest, ApiResponse } from './types/api';
import Login from './components/Login';
import './App.css';

function App() {
  const [connection, setConnection] = useState<TypedHubConnection | null>(null);
  const [gameState, setGameState] = useState<GameState | null>(null);
  const [connectionStatus, setConnectionStatus] = useState<string>('Disconnected');
  const [village, setVillage] = useState<Village | null>(null);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [username, setUsername] = useState<string | null>(null);

  useEffect(() => {
    // Always start with login screen for development
  }, []);

  useEffect(() => {
    if (isLoggedIn) {
      const newConnection = new HubConnectionBuilder()
        .withUrl('http://localhost:5033/gamehub')
        .build() as TypedHubConnection;

      setConnection(newConnection);
    }
  }, [isLoggedIn]);

  useEffect(() => {
    if (connection && isLoggedIn) {
      connection.start()
        .then(() => {
          console.log('Connected to Game Server!');
          setConnectionStatus('Connected');

          connection.on(GameClientMethods.GameStateUpdate, (state: GameState) => {
            setGameState(state);
          });

          connection.on(GameClientMethods.NotificationUpdate, (message: string) => {
            console.log('Notification:', message);
          });
        })
        .catch(e => {
          console.log('Connection failed: ', e);
          setConnectionStatus('Failed');
        });
    }
  }, [connection, isLoggedIn]);

  useEffect(() => {
    // Load village data from API after login
    if (isLoggedIn) {
      fetch('http://localhost:3001/api/game/village/1')
        .then(res => res.json())
        .then((data: Village) => setVillage(data))
        .catch(err => console.error('Failed to load village:', err));
    }
  }, [isLoggedIn]);

  const buildBarracks = async () => {
    try {
      const request: BuildBuildingRequest = { buildingType: 'Barracks' };
      
      const response = await fetch('http://localhost:3001/api/game/village/1/build', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      });
      
      if (response.ok) {
        const result: ApiResponse = await response.json();
        console.log('Build command sent successfully:', result.message);
      }
    } catch (error) {
      console.error('Failed to send build command:', error);
    }
  };

  const recruitSpearmen = async () => {
    try {
      const request: RecruitTroopsRequest = { 
        troopType: 'Spearman', 
        quantity: 5 
      };
      
      const response = await fetch('http://localhost:3001/api/game/village/1/recruit', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      });
      
      if (response.ok) {
        const result: ApiResponse = await response.json();
        console.log('Recruit command sent successfully:', result.message);
      }
    } catch (error) {
      console.error('Failed to send recruit command:', error);
    }
  };

  const handleLogin = (loggedInUsername: string) => {
    setUsername(loggedInUsername);
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    setUsername(null);
    setIsLoggedIn(false);
    setVillage(null);
    if (connection) {
      connection.stop();
    }
  };

  if (!isLoggedIn) {
    return <Login onLogin={handleLogin} />;
  }

  return (
    <div className="App">
      <header className="App-header">
        <h1>Villagers</h1>
        
        <div style={{ marginBottom: '20px' }}>
          <p>Player: <strong>{username}</strong> | <button onClick={handleLogout} style={{ marginLeft: '10px' }}>Logout</button></p>
          <p>Game Server: <strong>{connectionStatus}</strong></p>
          {gameState && (
            <p>Game Tick: <strong>{gameState.tick}</strong></p>
          )}
        </div>

        {village && (
          <div style={{ marginBottom: '20px', textAlign: 'left' }}>
            <h3>{village.name}</h3>
            <p>Wood: {village.resources.wood}</p>
            <p>Clay: {village.resources.clay}</p>
            <p>Iron: {village.resources.iron}</p>
          </div>
        )}

        <div>
          <button onClick={buildBarracks} style={{ margin: '10px' }}>
            Build Barracks
          </button>
          <button onClick={recruitSpearmen} style={{ margin: '10px' }}>
            Recruit 5 Spearmen
          </button>
        </div>

        <p style={{ fontSize: '14px', marginTop: '20px' }}>
          Commands go through Lambda API → Game Server<br/>
          Real-time updates come from Game Server via SignalR
        </p>
      </header>
    </div>
  );
}

export default App;
