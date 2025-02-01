import { UserBasicEntity } from "../types/userBasicEntity";

interface CacheEntry {
  user: UserBasicEntity;
  timestamp: number;
}

class UserCache {
  private cache: Map<string, CacheEntry> = new Map();
  private readonly maxAge: number = 5 * 60 * 1000; // 5 minutes in milliseconds

  set(href: string, user: UserBasicEntity): void {
    this.cache.set(href, {
      user,
      timestamp: Date.now(),
    });
  }

  get(href: string): UserBasicEntity | undefined {
    const entry = this.cache.get(href);
    if (!entry) return undefined;

    // Check if cache entry is still valid
    if (Date.now() - entry.timestamp > this.maxAge) {
      this.cache.delete(href);
      return undefined;
    }

    return entry.user;
  }

  clear(): void {
    this.cache.clear();
  }
}

export const userCache = new UserCache();
