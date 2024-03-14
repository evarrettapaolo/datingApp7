import { Component, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { UserParams } from 'src/app/_models/userParams';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[] | undefined;
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  genderList = [{ value: 'male', display: 'Males' }, { value: 'female', display: 'Females' }]

  constructor(private memberService: MembersService) {
    //copy the state of the memberService property
    this.userParams = memberService.getUserParams();  
  }

  ngOnInit(): void {
    this.loadMembers();   
  }

  loadMembers() {
    if (this.userParams) {
      this.memberService.setUserParams(this.userParams); //set memberService property
      this.memberService.getMembers(this.userParams).subscribe({
        next: response => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      })
    }
  }

  resetFilters() {
    //reset the memberService property
    this.userParams = this.memberService.resetUserParams(); 
    this.loadMembers();
  }

  pageChanged(event: any) {
    //check to make sure event.page is updated
    if (this.userParams && this.userParams?.pageNumber !== event.page) {
      this.userParams.pageNumber = event.page; //update the pageNumber local property
      this.memberService.setUserParams(this.userParams); //update the memberService property
      this.loadMembers(); //reload, create a new request
    }
  }

}
