import { EntityBase } from './entityBase'
import { HrefEntity } from './hrefEntity'

export interface Movie extends EntityBase<number> {
    imdbUrl?: string
    youtubeUrl?: string
    actor?: string
    reason?: string
    movieKillCount?: number
    hardheadKillCount?: number
    hardhead: HrefEntity
    posterPhoto?: HrefEntity
} 