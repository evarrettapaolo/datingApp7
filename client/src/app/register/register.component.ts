import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  model: any = {}

  constructor(private accountService: AccountService, private toastr: ToastrService) { }
  ngOnInit(): void {
    
  }

  //send request to API then exit
  register() {
    this.accountService.register(this.model).subscribe({
      next: response => {
        this.cancel();
      },
      error: error => {
        this.toastr.error('invalid entry');
      }
    })
  }
  
  //exit the register form
  cancel() {
    this.cancelRegister.emit(false);
  }

}
