function [positionState,a_hl_f,a_hr_f,a_kl_f,a_kr_f]=gait_finish(gait_length,gait_height,gait_length_normal,gait_length_change)
t=0:0.005:2; %采样时间
l1=0.4; l2=0.4;%l1是大腿长度，l2是小腿长度
zf=gait_height; %zc为变化后的步高
sf=gait_length/2; %s为步长
T=2;   %T为步态周期长度
vf=sf/(T/2);

sb=gait_length_normal/2;
s=gait_length_normal;
sc=gait_length_change;
positionState = 'finish';
for i=1:201
xa_f(i)=vf*t(i); %xa,ya为右脚位置
ya_f(i)=(zf/2)*sin((2*pi/(sf))*(xa_f(i))-(pi/2))+(zf/2);
x0_f(i)=(vf/2)*t(i)+sf/2;%x0,y0为髋关节位置
y0_f(i)=(((l1+l2)-sqrt((l1+l2)^2-(sf/2)^2))/2)*sin((2*pi/sf)*(x0_f(i)-sf/2)-(pi/2))+(l1+l2+sqrt((l1+l2)^2-(sf/2)^2))/2;
xc_f(i)=sf;%xc,yc为左脚位置
yc_f(i)=0;
end 
for i=1:201 %求解左膝关节位置
l=sqrt((xc_f(i)-x0_f(i))^2+(yc_f(i)-y0_f(i))^2);
    theat1=acos((l2^2+l^2-l1^2)/(2*l*l2));
    theat2=atan((y0_f(i)-yc_f(i))/(x0_f(i)-xc_f(i)));
    if(xc_f(i)>x0_f(i))
    x3=real(xc_f(i)-l2*cos(theat2-theat1));
    x4=real(xc_f(i)-l2*cos(theat2+theat1));
    y3=real(yc_f(i)+abs(l2*sin(theat2-theat1)));
    y4=real(yc_f(i)+abs(l2*sin(theat2+theat1)));
    else
     x3=real(xc_f(i)+l2*cos(theat2-theat1));
    x4=real(xc_f(i)+l2*cos(theat2+theat1));
    y3=real(yc_f(i)+abs(l2*sin(theat2-theat1)));
    y4=real(yc_f(i)+abs(l2*sin(theat2+theat1)));   
    end
    xd_f(i)=max(x3,x4);
    yd_f(i)=max(y3,y4);
end
for i=1:201 %求解右膝关节位置
l=sqrt((xa_f(i)-x0_f(i))^2+(ya_f(i)-y0_f(i))^2);
    theat1=acos((l2^2+l^2-l1^2)/(2*l*l2));
    theat2=atan((y0_f(i)-ya_f(i))/(x0_f(i)-xa_f(i)));
    if(xa_f(i)<x0_f(i))
    x1=real(xa_f(i)+l2*cos(theat2-theat1));
    x2=real(xa_f(i)+l2*cos(theat2+theat1));
    y1=real(ya_f(i)+abs(l2*sin(theat2-theat1)));
    y2=real(ya_f(i)+abs(l2*sin(theat2+theat1)));
    else
       x1=real(xa_f(i)-l2*cos(theat2-theat1));
    x2=real(xa_f(i)-l2*cos(theat2+theat1));
    y1=real(ya_f(i)+abs(l2*sin(theat2-theat1)));
    y2=real(ya_f(i)+abs(l2*sin(theat2+theat1))); 
    end
    xb_f(i)=max(x1,x2);
    yb_f(i)=max(y1,y2);
end
for i=1:201
a_hl_f(i)=atan((xd_f(i)-x0_f(i))/((y0_f(i)-yd_f(i))));
a_hl_f(i)=a_hl_f(i)*180/pi;
end
for i=1:201
a_hr_f(i)=atan((xb_f(i)-x0_f(i))/((y0_f(i)-yb_f(i))));
a_hr_f(i)=a_hr_f(i)*180/pi;
end
for i=1:201
a_kr_f(i)=atan((xa_f(i)-xb_f(i))/((yb_f(i)-ya_f(i))));
a_kr_f(i)=a_hr_f(i)-(a_kr_f(i)*180/pi);
end
for i=1:201
a_kl_f(i)=atan((xc_f(i)-xd_f(i))/((yd_f(i)-yc_f(i))));
a_kl_f(i)=a_hl_f(i)-(a_kl_f(i)*180/pi);
end
end