import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {

    //manipulate the observable object
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if(error) {
          switch(error.status) {
            case 400: //bad request error
              const modelStateErrors = [];
              if(error.error.errors) { //validation bad request
                for(const key in error.error.errors) {
                  if(error.error.errors[key]) {
                    modelStateErrors.push(error.error.errors[key]);
                  }
                }
                this.toastr.error('bad request due to validation error', error.status.toString());
                throw modelStateErrors.flat(); //make the error known
              }
              else { //intentionally create bad request
                this.toastr.error(error.error, error.status.toString());
              }
              break;
            case 401: //unauthorized error
              // this.toastr.error('Unauthorized', error.status.toString());
              console.log('401 unauthorized');
              break;
            case 404: //not found
              this.router.navigateByUrl('/not-found');
              break;
            case 500: //internal server error
              //include an object state upon rerouting
              const navigationExtras: NavigationExtras = {state: {error: error.error}}
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error('Something unexpected went wrong.');
              console.log(error);
              break;
          }
        }
        throw error;
      })
    )

  }
}
