import { UserBasicEntity } from './userBasicEntity';
import { TypeEntity } from './typeEntity';

export interface WinnerEntity {
  id?: number;
  winnerUserId: number;
  winner: UserBasicEntity;
  position: number;
  year: number;
  value?: number | null;
  measurementType?: TypeEntity | null;
  text: string;
}
