
<!--#include file="head_metronic.asp"-->
<!--#include file="navigation.asp"-->
					<!--begin::Content-->
					<div class="content d-flex flex-column flex-column-fluid" id="kt_content">
						<!--begin::Post-->
						<div class="" id="kt_post">
							<!--begin::Container-->
							<div id="kt_content_container" class="container-fluid">
								<h1 class="mb-3">Let's make this day productive!</h1>
								
								<div class="row">
									<div class="col-md-6 col-lg-6">
										<h1 class=" mb-3">My Lesson Plan</h1>
									</div>
									<div class="col-md-6 col-lg-6">
										<h1 class="mb-3 text-center">Today's Plan</h1>
									</div>
								</div>
								<div class="row g-5 g-xl-10 mb-5 mb-xl-10">
									
									<!--begin::Col-->
									<div class="col-md-6 col-lg-6 col-xl-6 col-xxl-3 mb-md-5 mb-xl-10">
										<!--begin::Card widget 20-->
											<a href="lessons_listing.asp?status=COMPLETED">
											<div class="card card-flush bgi-no-repeat bgi-size-contain bgi-position-x-end h-md-50 mb-5 mb-xl-10"  style="background-color: #31b5ff;">
									
											<!--begin::Card body-->
												<div class="d-flex flex-center flex-column flex-column-fluid p-10 pb-lg-20">
														<img alt="Logo" src="resources/completed logo.png" class="h-80px" />
													<div class="text-center mb-10">
														<!--begin::Title-->
														<h1 class="text-light mb-3">Completed</h1>
														<!--end::Title-->
													</div>
												</div>
											<!--end::Card body-->
											</div>
											</a>
										<!--end::Card widget 20-->
										<!--begin::Card widget 7-->
										<a href="lessons_listing.asp?status=CANCELED">
											<div class="card card-flush h-md-50 mb-5 mb-xl-10"    style="background-color: #ff688d;">
												<div class="d-flex flex-center flex-column flex-column-fluid p-10 pb-lg-20">
													<!--begin::Logo-->
														<img alt="Logo" src="resources/cancel logo.png" class="h-80px" />
													<!--end::Logo-->
													<div class="text-center mb-10">
														<!--begin::Title-->
														<h1 class="text-light mb-3">Canceled</h1>
														<!--end::Title-->
													</div>
												</div>
											</div>
										</a>
										<!--end::Card widget 7-->
									</div>
									<!--end::Col-->
									<!--begin::Col-->
									<div class="col-md-6 col-lg-6 col-xl-6 col-xxl-3 mb-md-5 mb-xl-10">
										<!--begin::Card widget 20-->
											<a href="lessons_listing.asp?status=PENDING" >
												<div class="card card-flush bgi-no-repeat bgi-size-contain bgi-position-x-end h-md-50 mb-5 mb-xl-10"    style="background-color: #ba9bfb;">
											
													<!--begin::Card body-->
														<div class="d-flex flex-center flex-column flex-column-fluid p-10 pb-lg-20">
																<img alt="Logo" src="resources/pending logo.png" class="h-80px" />
															<div class="text-center mb-10">
																<!--begin::Title-->
																<h1 class="text-light mb-3">Pending</h1>
																<!--end::Title-->
															</div>
														</div>
													<!--end::Card body-->
												</div>
											</a>
										<!--end::Card widget 20-->
										<!--begin::Card widget 7-->
										<a href="courses.asp">
											<div class="card card-flush h-md-50 mb-5 mb-xl-10"  style="background-color: #33e986;">
												<div class="d-flex flex-center flex-column flex-column-fluid p-10 pb-lg-20">
														<img alt="Logo" src="resources/create logo.png" class="h-80px" />
													<div class="text-center mb-10">
														<!--begin::Title-->	
														<h1 class="text-light mb-3">Create a Plan</h1>
														<!--end::Title-->
													</div>
												</div>
											</div>
										</a>
										<!--end::Card widget 7-->
									</div>
									<!--end::Col-->
									<!--begin::Col-->
									<div class="col-xxl-6" id="lessons" style="height: 70vh; overflow: auto;">

									</div>
									<!--end::Col-->
								</div>
							</div>
							<!--end::Container-->
						</div>
						<!--end::Post-->
					</div>
					<!--end::Content-->
				</div>
				<!--end::Wrapper-->
			</div>
			<!--end::Page-->
		</div>
		<!--end::Root-->
<script src="dashboard.js?t=<% Write(nocache_time) %>"></script>
        <% FooterIndicator = False %>
            <!--#include file="footer_metronic.asp"-->
