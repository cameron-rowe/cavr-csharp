sim_window = {
  view = {
    simulator_view = true;
  };
fullscreen = false;
};


x11_renderer = {
  type = "x11gl";
  display = ":0.0";
  windows = {
    sim_window = sim_window;
    --sim_window2 = sim_window;

  };
};

self = {
  hostname = HOSTNAME;
  ssh = HOSTNAME;--"chase@" .. HOSTNAME;
  address = HOSTNAME;
  plugins = {
    x11_renderer = x11_renderer;
    vrpn = vrpn;
  };
};



machines = {
  self=self;
  --self2 = others;
  --self3 = others2;
};
