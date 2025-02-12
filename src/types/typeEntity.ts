import { EntityBase } from './entityBase';

export interface TypeEntity extends EntityBase<number> {
  name?: string;
  description?: string;
}
