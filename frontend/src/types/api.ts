// Authentication types
export interface Player {
  id: string;
  username: string;
  registeredWorldIds: number[];
  createdAt: string;
  updatedAt: string;
}

export interface AuthResponse {
  token: string;
  player: Player;
  expiresAt: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
}

// API response wrapper
export interface ApiResponse<T = any> {
  message?: string;
  data?: T;
  success?: boolean;
}

// Error response
export interface ErrorResponse {
  message: string;
  details?: string;
}

// Health check
export interface HealthResponse {
  status: string;
  timestamp: string;
}

// SignalR command types (for future use)
export interface TestCommand {
  playerId: string;
  message: string;
}

// World state (for future use)
export interface WorldState {
  name: string;
  tickNumber: number;
  timestamp: string;
  message?: string;
}

// Game mechanics types (for future expansion)
export interface World {
  id: number;
  name: string;
  playerCount: number;
  maxPlayers: number;
  created: string;
}

export interface Village {
  id: string;
  name: string;
  playerId: string;
  worldId: number;
  x: number;
  y: number;
  population: number;
  resources: Resources;
  buildings: Building[];
}

export interface Resources {
  wood: number;
  clay: number;
  iron: number;
  food: number;
}

export interface Building {
  id: string;
  type: BuildingType;
  level: number;
  x: number;
  y: number;
}

// Building types
export type BuildingType = 
  | 'Headquarters' 
  | 'Barracks' 
  | 'Stable' 
  | 'Workshop' 
  | 'Farm' 
  | 'Warehouse' 
  | 'Wall';

// Troop types  
export type TroopType = 
  | 'Spearman' 
  | 'Swordsman' 
  | 'Axeman' 
  | 'Scout' 
  | 'Light Cavalry' 
  | 'Heavy Cavalry' 
  | 'Ram' 
  | 'Catapult';