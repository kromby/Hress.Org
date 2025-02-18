import { EntityBase } from "./entityBase";
import { HrefEntity } from "./hrefEntity";
import { YearEntity } from "./yearEntity";

export interface Award extends EntityBase<number> {
  winners: HrefEntity;
  href?: string;
  years: YearEntity[];
}
