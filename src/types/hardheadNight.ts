import { EntityBase } from './entityBase'
import { UserBasicEntity } from './userBasicEntity'
import { Movie } from './movie'
import { HrefEntity } from './hrefEntity'

export interface HardheadNight extends EntityBase<number> {
    date: string
    dateString: string
    number: number
    guestCount: number
    host: UserBasicEntity
    movie?: Movie
    year: HrefEntity
    nextHostId?: number
} 