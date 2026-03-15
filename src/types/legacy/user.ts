import { EntityBase } from '../entityBase';

export interface User extends EntityBase<number> {
    id: number;
    name: string;
    username: string;
}
