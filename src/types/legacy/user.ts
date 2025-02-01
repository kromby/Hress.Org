import { EntityBase } from '../entityBase';

export interface User extends EntityBase<number> {
    ID: number;
    Name: string;
}
