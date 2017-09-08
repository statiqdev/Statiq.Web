# 🏆 – A Blog Theme for Jekyll
Trophy is a blog theme for Jekyll built using HTML, Sass, and JavaScript (no jQuery). External stylesheets and libraries included are Google Fonts, Font Awesome, Normalize.CSS, Rellax, and WOW.js.
Trophy is also available in [Ghost](https://github.com/thomasvaeth/trophy-ghost).

## Screenshots
Blog Page (Posts)
![Blog Page](https://github.com/thomasvaeth/trophy-jekyll/blob/master/_screenshots/screenshot-1.png "Desktop screenshot")

Blog Page (Categories)
![Blog Page](https://github.com/thomasvaeth/trophy-jekyll/blob/master/_screenshots/screenshot-2.png "Desktop screenshot")

Post Page (Mast)
![Post Page](https://github.com/thomasvaeth/trophy-jekyll/blob/master/_screenshots/screenshot-3.png "Desktop screenshot")

Post Page (Profile & Footer)
![Post Page](https://github.com/thomasvaeth/trophy-jekyll/blob/master/_screenshots/screenshot-4.png "Desktop screenshot")

Archive Page
![Archive Page](https://github.com/thomasvaeth/trophy-jekyll/blob/master/_screenshots/screenshot-5.png "Desktop screenshot")

## Installation
All dependencies are saved in the ````Gemfile````. Run ````bundle install```` (Install [Bundler](http://bundler.io/) if it is not already) after cloning the repo.

## Edit Theme
I made everything as easy as possible to edit. Most things can be found in the ````_config.yml````, but if more editing is required digging through the code will be required. The ````head.html```` file is in the ````_includes```` folder and the Sass variables are found in the ````_base.scss```` file in the ````_sass```` folder.

### _config.yml

#### Site Settings
    email: 
    baseurl: ""
    paginate: 5
    paginate_path: "/blog/page-:num"
    google_analytics: UA—XXXXXXXX-X

* ````email```` - Your email for the contact card and the footer
* ````baseurl```` - Path of blog if adding this on to another website
* ````paginate```` - Number of blog posts per page
* ````paginate_path```` - URL structure of paginated pages
* ````google_analytics```` - Option field to replace with correct Google Analytics code

#### SEO Settings
    title: 
    description: 
    url: ""
    twitter_username: 
    default_img: 

* ````title```` - Title of blog
* ````description```` - Description of blog (recommended to not go over 160 characters)
* ````url```` - URL of main website
* ````twitter_username```` - Twitter username
* ````default_img```` - Image that will appear when posting links on social networks

#### Profile Settings
    name: 
    profile_img: 
    profile: 
    social:
      github: 

* ````name```` - Full name for SEO purposes
* ````profile_img```` - Image for the profile card (size to 2000x1200px)
* ````profile```` - Short description that will be in the profile card
* ````social```` - List of social networks for icons in the contact card and the footer ([Font Awesome](http://fontawesome.io/) is used, so only match the name of the icon, but do not include ````fa-````)


#### Build Settings
    exclude: ["_screenshots", "Gemfile", "Gemfile.lock", "LICENSE.txt", "README.md"]
    permalink: /:year/:month/:day/:title/

* ````exclude```` - Folders that are excluded from `_site_`
* ````permalink```` - URL structure of blog posts

### _posts
    ---
    layout: post
    title: ""
    date: 
    categories:
    description: 
    image: 
    image-sm:
    ---

This is the YAML front matter block for blog posts.
* ````layout```` - This field will always be post
* ````title```` - The title of the blog post
* ````date```` - The date that will appear on the blog post
* ````categories```` - Optional field that can be entered as an array or a list
* ````description```` - Optional field for SEO (recommended to not go over 160 characters)
* ````image```` - The blog theme was designed for 2000x1200px images (optimize your images because this is a picture heavy theme)
* ````image-sm```` - Optional field for card layouts for image optimization and page speed (designed for 500x300px images)

## Upcoming Additions
* Page transitions
* More styled elements for blog posts

## Issues
Please submit any issues [here](https://github.com/thomasvaeth/trophy-jekyll/issues).

## License
Trophy is licensed under the MIT License.
