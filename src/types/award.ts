import { EntityBase } from "./entityBase";
import { HrefEntity } from "./hrefEntity";

export interface Award extends EntityBase<number> {
    winners: HrefEntity
    href?: string
} 