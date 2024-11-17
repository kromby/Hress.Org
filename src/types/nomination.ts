import { EntityBase } from './entityBase';
import { HrefEntity } from './hrefEntity';

export interface Nomination extends EntityBase<string> {
    typeId: number;
    nominee: UserBasic;
    affectedRule?: string;
}

export interface UserBasic {
    id: number;
    name: string;
    profilePhoto?: HrefEntity;
} 
