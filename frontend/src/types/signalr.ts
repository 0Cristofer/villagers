// TypeScript interfaces matching the backend SignalR interfaces

export interface GameState {
  tick: number;
  timestamp: string;
}

export interface Village {
  id: number;
  name: string;
  resources: Resources;
}

export interface Resources {
  wood: number;
  clay: number;
  iron: number;
}

export interface CombatResult {
  attackerId: string;
  defenderId: string;
  result: 'victory' | 'defeat' | 'draw';
  losses: {
    attacker: number;
    defender: number;
  };
}

// Method name constants to avoid typos
export const GameClientMethods = {
  GameStateUpdate: 'GameStateUpdate',
  ResourceUpdate: 'ResourceUpdate', 
  CombatUpdate: 'CombatUpdate',
  NotificationUpdate: 'NotificationUpdate'
} as const;

// Simple type for SignalR connection
export type TypedHubConnection = {
  start(): Promise<void>;
  stop(): Promise<void>;
  on(methodName: string, newMethod: (...args: any[]) => void): void;
  off(methodName: string): void;
};