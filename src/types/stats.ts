export enum PeriodType {
  All,
  Last10,
  Last5,
  Last2,
  ThisYear
}

export interface StatisticBase {
  attendedCount: number;
  firstAttended: string;
  firstAttendedString: string;
  lastAttended: string;
  lastAttendedString: string;
}

export interface StatsEntity {
  periodType: PeriodType;
  periodTypeName: string;
  typeName: string;
  dateFrom: string;
  dateFromString: string;
  list: StatisticBase[];
}
