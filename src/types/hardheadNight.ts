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
    /** Where the night is held. */
    location?: string | null
    /** Start time as "20:00", or null when not set. */
    startTimeString?: string | null
} 