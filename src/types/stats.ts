export enum PeriodType {
    All = 'all',
    Last10 = 'last10',
    Last5 = 'last5',
    Last2 = 'last2',
    ThisYear = 'thisYear'
}

export interface StatisticBase {
  attendedCount: number;
  firstAttended: Date;
  firstAttendedString: string;
  lastAttended: Date;
  lastAttendedString: string;
}

export interface StatsEntity {
  periodType: PeriodType;
  periodTypeName: string;
  typeName: string;
  dateFrom: Date;
  dateFromString: string;
  list: StatisticBase[];
}
