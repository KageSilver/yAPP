<script setup>
	import { get } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted, ref } from "vue";
	import { useRouter } from "vue-router";
	import LoadingScreen from "../components/LoadingScreen.vue";
	import PostCard from "../components/PostCard.vue";

	const router = useRouter(); // Use router hook
	const uid = ref("");
	const username = ref("");
	const userDiaries = ref([]);
	const friendDiaries = ref([]);
	const loading = ref(false);

	var today = new Date();
	today.setHours(0, 0, 0, 0);
	var selectedDate = today;
	var friends = new Array();
	var friendUsernames = new Array();
	var friendDiariesArr = new Array();

	onMounted(async () => {
		loading.value = true;
		setCalendar();
		const user = await getCurrentUser();
		uid.value = user.userId;
		username.value = user.username;
		await getFriendsByStatus(username);
		await getFriendUIDs(username);
		await getUserDiaries(uid);
		await getFriendDiaries(uid);

	});

	// get list of accepted friends for their usernames
	async function getFriendsByStatus(username) {
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/friends/getFriendsByStatus?userName=${username.value}&status=1`,
			});
			const { body } = await restOperation.response;
			const response = await (await body.blob()).arrayBuffer();
			const decoder = new TextDecoder("utf-8"); // Use TextDecoder to decode the ArrayBuffer to a string
			const decodedText = decoder.decode(response);
			friends = JSON.parse(decodedText); // Update with parsed JSON
		} catch (error) {
			console.log("GET call failed", error);
		}
	}

	// get uids for all friends to attach their username to non-anonymous diary entries
	async function getFriendUIDs(username) {
		for (let i = 0; i < friends.length; i++) {
			var friendUsername;

			if (friends[i].FromUserName == username.value) {
				friendUsername = friends[i].ToUserName;
			} else {
				friendUsername = friends[i].FromUserName;
			}

			try {
				const restOperation = get({
					apiName: "yapp",
					path: `/api/users/getUserByName?userName=${friendUsername}`,
				});

				const { body } = await restOperation.response;
				const response = await (await body.blob()).arrayBuffer();
				const decoder = new TextDecoder("utf-8"); // Use TextDecoder to decode the ArrayBuffer to a string
				const decodedText = decoder.decode(response);
				var thisFriend = JSON.parse(decodedText); // Update with parsed JSON

				var friendInfo = { userName: friendUsername, uid: thisFriend.id };
				friendUsernames.push(friendInfo);
			} catch (error) {
				console.log("GET call failed", error);
			}
		}
	}

	// gets diaries from the current user by date
	async function getUserDiaries(uid) {
		loading.value = true;
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/posts/getDiariesByUser?uid=${uid.value}&current=${selectedDate.toISOString()}`,
			});
			const { body } = await restOperation.response;
			const response = await (await body.blob()).arrayBuffer();
			const decoder = new TextDecoder("utf-8"); // Use TextDecoder to decode the ArrayBuffer to a string
			const decodedText = decoder.decode(response);
			userDiaries.value = JSON.parse(decodedText); // Update with parsed JSON
		} catch (e) {
			console.log("GET call failed", e);
		}
	}

	// gets diaries from the current user's friends by date
	async function getFriendDiaries(uid) {
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/posts/getDiariesByFriends?uid=${uid.value}&current=${selectedDate.toISOString()}`,
			});
			const { body } = await restOperation.response;
			const response = await (await body.blob()).arrayBuffer();
			const decoder = new TextDecoder("utf-8"); // Use TextDecoder to decode the ArrayBuffer to a string
			const decodedText = decoder.decode(response);
			friendDiariesArr = JSON.parse(decodedText); // Update with parsed JSON

			getUsernamesForPosts();
		} catch (e) {
			console.log("GET call failed", e);
		}
		loading.value = false;
	}

	// attaches usernames to friends diary posts if non-anonymous
	function getUsernamesForPosts() {
		for (let i = 0; i < friendDiariesArr.length; i++) {
			if (friendDiariesArr[i].anonymous) {
				friendDiariesArr[i].username = "Anonymous";
			} else {
				var match = friendUsernames.filter(function (friend) {
					return friend.uid == friendDiariesArr[i].uid;
				});
				friendDiariesArr[i].username = match[0].userName;
			}
		}

		friendDiaries.value = friendDiariesArr;
	}

	// sets the layout of the calendar
	function setCalendar() {
		var daysInMonth = new Date(
			selectedDate.getFullYear(),
			selectedDate.getMonth() + 1,
			0,
		).getDate();
		var blankDays = new Date(
			selectedDate.getFullYear(),
			selectedDate.getMonth(),
		).getDay();

		resetMonthPicker();
		changeDateHeader();
		moveFirstDay(blankDays);
		adjustDaysInMonth(daysInMonth);
	}

	// resets the calendar for changing the month
	async function reset() {
		setCalendar();
		await getUserDiaries(uid);
		await getFriendDiaries(uid);
	}

	// adjusts where the first day of the month is located
	function moveFirstDay(blankDays) {
		for (var i = 0; i < 6; i++) {
			var string = "blank" + i;
			var blank = document.getElementById(string);

			if (i < blankDays) {
				blank.style.display = "block";
			} else {
				blank.style.display = "none";
			}
		}
	}

	// adjusts the number of days displayed in the month
	function adjustDaysInMonth(daysInMonth) {
		if (daysInMonth == 28) {
			document.getElementById("29").style.display = "none";
		}

		if (daysInMonth <= 29) {
			document.getElementById("30").style.display = "none";
		}

		if (daysInMonth <= 30) {
			document.getElementById("31").style.display = "none";
		} else {
			document.getElementById("29").style.display = "block";
			document.getElementById("30").style.display = "block";
			document.getElementById("31").style.display = "block";
		}
	}

	// sets the month displayed above the calendar based on the selected date/month
	function resetMonthPicker() {
		var monthPicker = document.getElementById("monthPicker");
		monthPicker.innerHTML =
			selectedDate.toLocaleString("default", { month: "long" }) +
			" " +
			selectedDate.getFullYear();
	}

	// moves the calendar to the previous month
	function prevMonth() {
		if (selectedDate.getMonth() == 1) {
			// go to dec prev year
			selectedDate = new Date(selectedDate.getFullYear() - 1, 12, 1);
		} else {
			// go to prev month
			selectedDate = new Date(
				selectedDate.getFullYear(),
				selectedDate.getMonth() - 1,
				1,
			);
		}
		selectedDate.setHours(0, 0, 0, 0);
		reset();
	}

	// moves the calendar to the next month
	function nextMonth() {
		if (selectedDate.getMonth() == 11) {
			// go to jan next year
			selectedDate = new Date(selectedDate.getFullYear() + 1, 1, 1);
		} else {
			// go to next month
			selectedDate = new Date(
				selectedDate.getFullYear(),
				selectedDate.getMonth() + 1,
				1,
			);
		}
		selectedDate.setHours(0, 0, 0, 0);
		reset();
	}

	// changes the date selected on the calendar and fetches diaries from that date
	async function changeSelectedDate(date) {
		userDiaries.value = null;
		friendDiaries.value = null;

		selectedDate = new Date(
			selectedDate.getFullYear(),
			selectedDate.getMonth(),
			date,
		);
		selectedDate.setHours(0, 0, 0, 0);
		changeDateHeader();

		await getUserDiaries(uid);
		await getFriendDiaries(uid);
	}

	// changes the header that displayed the current selected date
	function changeDateHeader() {
		document.getElementById("date").innerHTML = selectedDate.toDateString();
	}

	// collapses the calendar
	function collapseCalendar() {
		if (calendar.style.display == "none") {
			document.getElementById("calendar").style.display = "block";
			document.getElementById("openCalendar").style.display = "none";
			document.getElementById("closeCalendar").style.display = "block";
		} else {
			document.getElementById("calendar").style.display = "none";
			document.getElementById("openCalendar").style.display = "block";
			document.getElementById("closeCalendar").style.display = "none";
		}
	}

	function clickPost(pid) {
		router.push({ name: "details", params: { pid } });
	}
</script>

<template>
	<div class="backBtnDiv">
		<!-- header -->
		<div class="flex items-center">
			<label id="date" class="flex text-lg font-bold text-white"></label>
			<button type="button" class="flex" @click="collapseCalendar">
				<img
					id="closeCalendar"
					src="../assets/calendar/closeCalendar.svg"
					style="display: block" />
				<img
					id="openCalendar"
					class="rotate-180"
					src="../assets/calendar/closeCalendar.svg"
					style="display: none" />
			</button>
		</div>

		<div id="calendar" style="display: block">
			<hr class="h-0.1 mx-auto mt-4 w-full bg-white" />

			<!-- month picker -->
			<div class="flex items-center">
				<button
					type="button"
					class="flex items-center justify-center p-1.5"
					@click="prevMonth">
					<img src="../assets/calendar/prevMonth.svg" />
				</button>
				<label id="monthPicker" class="m-4 flex font-bold text-white"></label>
				<button
					type="button"
					class="flex items-center justify-center p-1.5"
					@click="nextMonth">
					<img src="../assets/calendar/nextMonth.svg" />
				</button>
			</div>

			<!-- calendar -->
			<div class="mt-3 w-full md:mx-6">
				<div class="grid grid-cols-7 text-center">
					<div class="font-bold text-white">S</div>
					<div class="font-bold text-white">M</div>
					<div class="font-bold text-white">T</div>
					<div class="font-bold text-white">W</div>
					<div class="font-bold text-white">T</div>
					<div class="font-bold text-white">F</div>
					<div class="font-bold text-white">S</div>
				</div>
				<div class="mt-4 grid grid-cols-7 gap-px">
					<template v-for="i in 6">
						<div
							:id="'blank' + (i - 1)"
							class="mx-auto flex size-10 w-full items-center justify-center"
							style="display: none"></div>
					</template>

					<template v-for="day in 31">
						<button
							:id="day"
							type="button"
							class="mx-auto flex size-10 w-full items-center justify-center text-white hover:rounded-full hover:border-2 focus:rounded-full focus:bg-[#EFB2CE] focus:text-black"
							@click="changeSelectedDate(day)">
							<time :datetime="'#_'">{{ day }}</time>
						</button>
					</template>
					
				</div>
			</div>
		</div>

		<hr class="h-0.1 mx-auto mb-8 mt-4 w-full bg-white" />

		<LoadingScreen v-if="loading" />
		
		<div class="mx-auto flex w-full flex-col items-center">
			<div
				class="card m-2 w-full max-w-4xl cursor-pointer rounded-lg border border-gray-300 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md"
				v-for="post in userDiaries"
				:key="post.pid"
				@click="clickPost(post.pid)">
				<PostCard :post="post" :isDiary="true" />
			</div>

			<div
				class="card m-2 w-full max-w-4xl cursor-pointer rounded-lg border border-gray-300 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md"
				v-for="post in friendDiaries"
				:key="post.pid"
				@click="clickPost(post.pid)">
				<PostCard :post="post" :isDiary="true"/>
			</div>
		</div>

		
	</div>
</template>
