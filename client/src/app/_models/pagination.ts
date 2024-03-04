export interface Pagination{
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

// allow to page other things
export class PaginatedResult<T> {
  result?: T; //used to store Pagination list of items
  pagination?: Pagination;
}