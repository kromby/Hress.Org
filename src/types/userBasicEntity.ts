import { EntityBase } from './entityBase'
import { HrefEntity } from './hrefEntity'

export interface UserBasicEntity extends EntityBase<number> {
    username?: string
    href: string
    profilePhoto?: HrefEntity
} 