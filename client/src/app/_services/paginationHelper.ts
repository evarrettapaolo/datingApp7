import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs";
import { PaginatedResult } from "../_models/pagination";

//pagination request, generic type
export function getPaginatedResult<T>(url: string, params: HttpParams, http: HttpClient) {
  const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>;
  return http.get<T>(url, { observe: 'response', params }).pipe(
    // if(this.members.length > 0) return of(this.members); //cache data in an array
    map(response => {
      if (response.body) {
        paginatedResult.result = response.body; //store in class array
      }
      const pagination = response.headers.get('Pagination'); //get the header for pagination from API
      if (pagination) {
        paginatedResult.pagination = JSON.parse(pagination);
      }
      return paginatedResult;
    })
  );
}

// request header query string
export function getPaginationHeaders(pageNumber: number, pageSize: number): HttpParams {
  let params = new HttpParams();

  params = params.append('pageNumber', pageNumber);
  params = params.append('pageSize', pageSize);

  return params;
}