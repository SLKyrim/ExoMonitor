clear all;


gait_length_normal=0.4;
gait_length_change_big=1.006;
gait_length_change_small=0.153;
gait_height_normal=0.09;
gait_height_change=0.177;
gait_height_finish=0.088;
gait_height_change_small=0.06;



[state,a_hl,a_hr,a_kl,a_kr]=gait_start(gait_length_normal,gait_height_normal);
[state,a_hl_n,a_hr_n,a_kl_n,a_kr_n]=gait_normal(gait_length_normal,gait_height_normal);
[state,a_hl_c_small,a_hr_c_small,a_kl_c_small,a_kr_c_small]=gait_change(gait_length_change_small,gait_height_change_small,gait_length_normal);%change的最后一个输入参数要和上一个函数的输入步长一致
[state,a_hl_c,a_hr_c,a_kl_c,a_kr_c]=gait_change(gait_length_change_big,gait_height_change,gait_length_change_small);%change的最后一个输入参数要和上一个函数的输入步长一致
[state,a_hl_f,a_hr_f,a_kl_f,a_kr_f]=gait_finish(gait_length_change_big,gait_height_finish,gait_length_normal,gait_length_change_big);%finish的最后一个输入参数要和上一个函数的输入步长一致
a_hl=-1*[a_hl,a_hl_n,a_hl_c_small,a_hl_c,a_hl_f];
a_hr=[a_hr,a_hr_n,a_hr_c_small,a_hr_c,a_hr_f];
a_kl=[a_kl,a_kl_n,a_kl_c_small,a_kl_c,a_kl_f];
a_kr=-1*[a_kr,a_kr_n,a_kr_c_small,a_kr_c,a_kr_f];

[B, A] = butter(2, [0.001* (2/200), 4* (2/200)]);
akl = filtfilt(B, A, a_kl); 
ahl = filtfilt(B, A, a_hl); 
ahr = filtfilt(B, A, a_hr); 
akr = filtfilt(B, A, a_kr); 
hold on;
plot(a_kl, 'b');
plot(akl, 'r');

plot(a_hl, 'b');
plot(ahl, 'r');

plot(a_hr, 'b');
plot(ahr, 'r');

plot(a_kr, 'b');
plot(akr, 'r');
grid on;
start_kl = akl(1:400);
start_hl = ahl(1:400);
start_hr = ahr(1:400);
start_kr = akr(1:400);


%保存数据
dat=zeros(200, 4);
dat(:, 1) = akl(1:200);
dat(:, 2)  = ahl(1:200);
dat(:, 3)  = ahr(1:200);
dat(:, 4)  = akr(1:200);
save('Step_1_start.txt','dat','-ascii');

dat=zeros(200, 4);
dat(:, 1) = akl(201:400);
dat(:, 2)  = ahl(201:400);
dat(:, 3)  = ahr(201:400);
dat(:, 4)  = akr(201:400);
save('Step_2_normal.txt','dat','-ascii');

dat=zeros(200, 4);
dat(:, 1) = akl(401:600);
dat(:, 2)  = ahl(401:600);
dat(:, 3)  = ahr(401:600);
dat(:, 4)  = akr(401:600);
save('Step_3_normal.txt','dat','-ascii');

dat=zeros(200, 4);
dat(:, 1) = akl(601:800);
dat(:, 2)  = ahl(601:800);
dat(:, 3)  = ahr(601:800);
dat(:, 4)  = akr(601:800);
save('Step_4_change_small.txt','dat','-ascii');

dat=zeros(200, 4);
dat(:, 1) = akl(801:1000);
dat(:, 2)  = ahl(801:1000);
dat(:, 3)  = ahr(801:1000);
dat(:, 4)  = akr(801:1000);
save('Step_5_change_small.txt','dat','-ascii');

dat=zeros(200, 4);
dat(:, 1) = akl(1001:1200);
dat(:, 2)  = ahl(1001:1200);
dat(:, 3)  = ahr(1001:1200);
dat(:, 4)  = akr(1001:1200);
save('Step_6_change_big.txt','dat','-ascii');

dat=zeros(200, 4);
dat(:, 1) = akl(1201:1400);
dat(:, 2)  = ahl(1201:1400);
dat(:, 3)  = ahr(1201:1400);
dat(:, 4)  = akr(1201:1400);
save('Step_7_change_big.txt','dat','-ascii');

dat=zeros(205, 4);
dat(:, 1) = akl(1401:1605);
dat(:, 2)  = ahl(1401:1605);
dat(:, 3)  = ahr(1401:1605);
dat(:, 4)  = akr(1401:1605);
save('Step_8_finish.txt','dat','-ascii');



