import React, { useState, useEffect } from 'react';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import './App.css';

interface WorldState {
  name: string;
  tickNumber: number;
  timestamp: string;
  message: string;
}

function App() {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [worldState, setWorldState] = useState<WorldState | null>(null);
  const [connectionStatus, setConnectionStatus] = useState<string>('Disconnected');
  const [message, setMessage] = useState<string>('');
  const [playerId, setPlayerId] = useState<string>('Player1');

  useEffect(() => {
    // Connect to game server SignalR hub
    const newConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5033/gamehub')
      .build();

    setConnection(newConnection);

    newConnection.start()
      .then(() => {
        console.log('Connected to Game Server!');
        setConnectionStatus('Connected');

        // Listen for world updates
        newConnection.on('WorldUpdate', (state: WorldState) => {
          console.log('Received world update:', state);
          setWorldState(state);
        });
      })
      .catch(e => {
        console.log('Connection failed: ', e);
        setConnectionStatus('Failed');
      });

    return () => {
      newConnection.stop();
    };
  }, []);

  const sendTestCommand = async () => {
    try {
      const request = {
        playerId: playerId,
        message: message
      };

      const response = await fetch('http://localhost:3001/api/command/test', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      });

      if (response.ok) {
        console.log('Test command sent successfully');
        setMessage(''); // Clear the input
      } else {
        console.error('Failed to send test command');
      }
    } catch (error) {
      console.error('Error sending test command:', error);
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Villagers Game</h1>
        
        <div style={{ marginBottom: '20px' }}>
          <p>Game Server: <strong>{connectionStatus}</strong></p>
        </div>

        {worldState && (
          <div style={{ marginBottom: '20px', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h3>World: {worldState.name}</h3>
            <p>Tick: {worldState.tickNumber}</p>
            <p>Message: <strong>{worldState.message || 'No message'}</strong></p>
            <p>Last Update: {new Date(worldState.timestamp).toLocaleTimeString()}</p>
          </div>
        )}

        <div style={{ marginBottom: '20px' }}>
          <h4>Send Test Command</h4>
          <div style={{ marginBottom: '10px' }}>
            <label>
              Player ID: 
              <input 
                type="text" 
                value={playerId} 
                onChange={(e) => setPlayerId(e.target.value)}
                style={{ marginLeft: '10px', padding: '5px' }}
              />
            </label>
          </div>
          <div style={{ marginBottom: '10px' }}>
            <label>
              Message: 
              <input 
                type="text" 
                value={message} 
                onChange={(e) => setMessage(e.target.value)}
                placeholder="Enter test message"
                style={{ marginLeft: '10px', padding: '5px', width: '200px' }}
              />
            </label>
          </div>
          <button 
            onClick={sendTestCommand}
            disabled={!message.trim()}
            style={{ 
              padding: '10px 20px', 
              backgroundColor: message.trim() ? '#007bff' : '#ccc',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: message.trim() ? 'pointer' : 'not-allowed'
            }}
          >
            Send Test Command
          </button>
        </div>

        <p style={{ fontSize: '14px', marginTop: '20px' }}>
          Commands: Frontend → API → Game Server<br/>
          Updates: Game Server → SignalR → Frontend
        </p>
      </header>
    </div>
  );
}

export default App;