// API request/response types

export interface BuildBuildingRequest {
  buildingType: string;
}

export interface RecruitTroopsRequest {
  troopType: string;
  quantity: number;
}

export interface ApiResponse<T = any> {
  message?: string;
  data?: T;
}

export interface HealthResponse {
  api: string;
  gameServer: string;
  timestamp: string;
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