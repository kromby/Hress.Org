export interface EntityBase<T> {
    id: T
    name?: string
    description?: string
    inserted: string
    insertedString: string
    insertedBy: number
    updated?: string
    updatedBy?: number
} 