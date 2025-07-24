create table Users (
id int primary key auto_increment,
username varchar(100) unique not null,
password_hash varchar(256) not null,
is_admin bit default 0);

create table Prompts (
id int primary key auto_increment,
question_text varchar(500) not null,
date_posted datetime default current_timestamp,
is_used bit default 0);

create table Feedback (
id int primary key auto_increment,
prompt_id int,
user_id int,
message text not null,
is_anonymous bit default 1,
date_submitted datetime default current_timestamp,
foreign key (prompt_id) references Prompts(id),
foreign key (user_id) references Users(id));
