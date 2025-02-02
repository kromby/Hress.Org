import { EntityBase } from "./entityBase";
import { HrefEntity } from "./hrefEntity";
import { UserBasicEntity } from "./userBasicEntity";

export interface YearEntity extends EntityBase<number> {
    guestCount: number;
    description: string;
    photoId?: number;
    photo?: HrefEntity;
    hardhead: UserBasicEntity;
}

export interface Award extends EntityBase<number> {
    winners: HrefEntity;
    href?: string;
    years: YearEntity[];
}