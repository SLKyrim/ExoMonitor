function [positionState,a_hl_c,a_hr_c,a_kl_c,a_kr_c]=gait_change(gait_length,gait_height,gait_length_normal)
t=0:0.005:2; %采样时间
l1=0.4; l2=0.4;%l1是大腿长度，l2是小腿长度
zc=gait_height; %zc为变化后的步高
sc=gait_length; %s为步长
T=2;   %T为步态周期长度
vc=sc/(T/2);

positionState = 'change';
s=gait_length_normal;
for i=1:101
xa_c(i)=(-2*(sc/2+s/2))*t(i)^3+(3*(sc/2+s/2))*t(i)^2; %xa,ya为右脚位置
ya_c(i)=(zc/2)*sin((2*pi/(sc/2+s/2))*(xa_c(i))-(pi/2))+(zc/2);
x0_c(i)=((-2*(s))*t(i)^3+(3*(s))*t(i)^2)/2+s/4;%x0,y0为髋关节位置
y0_c(i)=(((l1+l2)-sqrt((l1+l2)^2-(s/4)^2))/2)*sin((2*pi/(s/2))*(x0_c(i)-s/4)-pi/2)+(l1+l2+sqrt((l1+l2)^2-(s/4)^2))/2;
xc_c(i)=s/2; %xc,yc为左脚位置
yc_c(i)=0;
end 
for i=101:201
xa_c(i)=(-2*(sc/2+s/2))*t(i)^3+(3*(sc/2+s/2))*t(i)^2; %xa,ya为右脚位置
ya_c(i)=(zc/2)*sin((2*pi/(sc/2+s/2))*(xa_c(i))-(pi/2))+(zc/2);
x0_c(i)=((-2*(sc))*t(i)^3+(3*(sc))*t(i)^2)/2+s/4-(sc-s)/4;%x0,y0为髋关节位置
y0_c(i)=(((l1+l2)-sqrt((l1+l2)^2-(sc/4)^2))/2)*sin((2*pi/(sc/2))*(x0_c(i)-s/2)+pi/2)+(l1+l2+sqrt((l1+l2)^2-(sc/4)^2))/2;
xc_c(i)=s/2; %xc,yc为左脚位置
yc_c(i)=0;
end 
for i=201:401
xa_c(i)=sc/2+s/2; %xa,ya为右脚位置
ya_c(i)=0;
x0_c(i)=((-2*sc)*(t(i)-1)^3+(3*sc)*(t(i)-1)^2)/2+s/4+sc/4+s/4;%x0,y0为髋关节位置
y0_c(i)=(((l1+l2)-sqrt((l1+l2)^2-(sc/4)^2))/2)*sin((2*pi/(sc/2))*(x0_c(i)-s/4-sc/4-s/4)-(pi/2))+(l1+l2+sqrt((l1+l2)^2-(sc/4)^2))/2;
xc_c(i)=s/2+(-2*sc)*(t(i)-1)^3+(3*sc)*(t(i)-1)^2; %xc,yc为左脚位置
yc_c(i)=(zc/2)*sin((2*pi/(sc))*(xc_c(i)-s/2)-(pi/2))+(zc/2);
end 
for i=1:401 %求解右膝关节位置
 l=sqrt((xa_c(i)-x0_c(i))^2+(ya_c(i)-y0_c(i))^2);
    theat1=acos((l2^2+l^2-l1^2)/(2*l*l2));
    theat2=atan((y0_c(i)-ya_c(i))/(x0_c(i)-xa_c(i)));
    if(xa_c(i)<x0_c(i))
        x1=real(xa_c(i)+l2*cos(theat2-theat1));
        x2=real(xa_c(i)+l2*cos(theat2+theat1));
        y1=real(ya_c(i)+abs(l2*sin(theat2-theat1)));
        y2=real(ya_c(i)+abs(l2*sin(theat2+theat1)));
    else
        x1=real(xa_c(i)-l2*cos(theat2-theat1));
        x2=real(xa_c(i)-l2*cos(theat2+theat1));
        y1=real(ya_c(i)+abs(l2*sin(theat2-theat1)));
        y2=real(ya_c(i)+abs(l2*sin(theat2+theat1))); 
    end
    xb_c(i)=max(x1,x2);
    yb_c(i)=max(y1,y2);
   
end
for i=1:401 %求解左膝关节位置
 l=sqrt((xc_c(i)-x0_c(i))^2+(yc_c(i)-y0_c(i))^2);
    theat1=acos((l2^2+l^2-l1^2)/(2*l*l2));
    theat2=atan((y0_c(i)-yc_c(i))/(x0_c(i)-xc_c(i)));
    if(xc_c(i)>x0_c(i))
    x3=real(xc_c(i)-l2*cos(theat2-theat1));
    x4=real(xc_c(i)-l2*cos(theat2+theat1));
    y3=real(yc_c(i)+abs(l2*sin(theat2-theat1)));
    y4=real(yc_c(i)+abs(l2*sin(theat2+theat1)));
    else
     x3=real(xc_c(i)+l2*cos(theat2-theat1));
    x4=real(xc_c(i)+l2*cos(theat2+theat1));
    y3=real(yc_c(i)+abs(l2*sin(theat2-theat1)));
    y4=real(yc_c(i)+abs(l2*sin(theat2+theat1)));   
 
    end
    xd_c(i)=max(x3,x4);
    yd_c(i)=max(y3,y4);
end
for i=1:401
a_hl_c(i)=atan((xd_c(i)-x0_c(i))/(y0_c(i)-yd_c(i)));
a_hl_c(i)=a_hl_c(i)*180/pi;
end
for i=1:401
a_hr_c(i)=atan((xb_c(i)-x0_c(i))/(y0_c(i)-yb_c(i)));
a_hr_c(i)=a_hr_c(i)*180/pi;
end
for i=1:401
a_kr_c(i)=atan((xa_c(i)-xb_c(i))/(yb_c(i)-ya_c(i)));
a_kr_c(i)=a_hr_c(i)-(a_kr_c(i)*180/pi);
if(a_kr_c(i)<0)
    a_kr_c(i)=0;
end
end
for i=1:401
a_kl_c(i)=atan((xc_c(i)-xd_c(i))/(yd_c(i)-yc_c(i)));
a_kl_c(i)=a_hl_c(i)-(a_kl_c(i)*180/pi);
if(a_kl_c(i)<0)
    a_kl_c(i)=0;
end
end
end