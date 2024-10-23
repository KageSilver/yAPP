<script setup>
    import { getCurrentUser } from 'aws-amplify/auth';
    import { DeprecationTypes, onMounted, ref } from 'vue';
    import { get } from 'aws-amplify/api';
    import { useRouter } from 'vue-router';

    const router = useRouter(); // Use router hook
    const uid = ref('');
    const jsonData = ref([]);
    const loading = false;

    var today = new Date();
    var selectedDate = today;

    // Retrieve the necessary data and function from the helper
    onMounted(async () => {
        resetCalendar();
        const user = await getCurrentUser();
        uid.value = user.userId;
        await getMyDiaryEntries(uid);
    });

    async function getMyDiaryEntries(uid) {
        try {
            const restOperation = get({
                apiName: 'yapp',
                path: `/api/posts/getPostsByUser?uid=${uid.value}&diaryEntry=${true}`
            });
            const { body } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
            const decodedText = decoder.decode(response);
            jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
        } catch (e) {
            console.log('GET call failed', error);
        }
    }

    function resetCalendar() {
        var daysInMonth = new Date(selectedDate.getFullYear(), selectedDate.getMonth() + 1, 0).getDate();
        var blankDays = new Date(selectedDate.getFullYear(), selectedDate.getMonth()).getDay();

        resetMonthPicker();
        changeDateHeader();
        moveFirstDay(blankDays);
        adjustDaysInMonth(daysInMonth);
    }

    function moveFirstDay(blankDays) {
        for(var i = 0; i < 6; i++){
            var string = "blank" + i;
            var blank = document.getElementById(string);

            if(i < blankDays){
                blank.style.display = 'block';
            }else{
                blank.style.display = 'none';
            }
        }
    }

    function adjustDaysInMonth(daysInMonth) {
        if(daysInMonth == 28) {
            document.getElementById("29").style.display = 'none';
        }

        if(daysInMonth <= 29) {
            document.getElementById("30").style.display = 'none';
        }

        if(daysInMonth <= 30) {
            document.getElementById("31").style.display = 'none';
        } else {
            document.getElementById("29").style.display = 'block';
            document.getElementById("30").style.display = 'block';
            document.getElementById("31").style.display = 'block';
        }
    }

    function resetMonthPicker() {
        var monthPicker = document.getElementById("monthPicker");
        monthPicker.innerHTML = selectedDate.toLocaleString('default', { month: 'long' }) + " " + selectedDate.getFullYear();
    }

    function prevMonth() {
        if(selectedDate.getMonth() == 1){
            // go to dec prev year
            selectedDate = new Date(selectedDate.getFullYear()-1, 12, 1);
        }else{
            // go to prev month
            selectedDate = new Date(selectedDate.getFullYear(), selectedDate.getMonth()-1, 1);
        }

        resetCalendar();
    }

    function nextMonth() {
        if(selectedDate.getMonth() == 11){
            // go to jan next year
            selectedDate = new Date(selectedDate.getFullYear()+1, 1, 1);
        }else{
            // go to next month
            selectedDate = new Date(selectedDate.getFullYear(), selectedDate.getMonth()+1, 1);
        }

        resetCalendar();
    }

    function changeSelectedDate(date) {
        selectedDate = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), date);
        changeDateHeader();
    }

    function changeDateHeader() {
        document.getElementById("date").innerHTML = selectedDate.toDateString();
    }

    function collapseCalendar() {
        var calendar = document.getElementById("calendar");
        var buttonIcon = document.getElementById("collapse");

        if(calendar.style.display == 'none') {
            calendar.style.display = 'block';
        } else {
            calendar.style.display = 'none';
        }
    }
</script>

<template> 

    <div class="backBtnDiv"> 
        <!-- header -->
        <div class="flex items-center">
            <label id="date" class="flex text-white font-bold text-lg"></label>
            <button type="button" class="flex" @click="collapseCalendar">
                <img id="collapse" src="../assets/calendar/closeCalendar.svg"></img>
            </button>
        </div>

        <div id="calendar" style="display:block">
            <hr class="w-full h-0.1 mx-auto mt-4 bg-white">

            <!-- month picker -->
            <div class="flex items-center">
                <button type="button"
                    class="flex items-center justify-center p-1.5"
                    @click="prevMonth">
                    <img src="../assets/calendar/prevMonth.svg"></img>
                </button>
                <label id="monthPicker" class="flex text-white font-bold m-4"></label>
                <button type="button"
                    class="flex items-center justify-center p-1.5"
                    @click="nextMonth">
                    <img src="../assets/calendar/nextMonth.svg"></img>
                </button>
            </div>

            <!-- calendar -->
            <div class="w-full md:mx-6 mt-3">
                <div class="grid grid-cols-7 text-center">
                    <div class="font-bold text-white">S</div>
                    <div class="font-bold text-white">M</div>
                    <div class="font-bold text-white">T</div>
                    <div class="font-bold text-white">W</div>
                    <div class="font-bold text-white">T</div>
                    <div class="font-bold text-white">F</div>
                    <div class="font-bold text-white">S</div>
                </div>
                <div class="mt-4 gap-px grid grid-cols-7">
                    <div id="blank0" class="mx-auto flex size-10 w-full items-center justify-center" style="display:none"></div>
                    <div id="blank1" class="mx-auto flex size-10 w-full items-center justify-center" style="display:none"></div>
                    <div id="blank2" class="mx-auto flex size-10 w-full items-center justify-center" style="display:none"></div>
                    <div id="blank3" class="mx-auto flex size-10 w-full items-center justify-center" style="display:none"></div>
                    <div id="blank4" class="mx-auto flex size-10 w-full items-center justify-center" style="display:none"></div>
                    <div id="blank5" class="mx-auto flex size-10 w-full items-center justify-center" style="display:none"></div>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(1)">
                        <time datetime="#_">1</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(2)">
                        <time datetime="#_">2</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(3)">
                        <time datetime="#_">3</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(4)">
                        <time datetime="#_">4</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(5)">
                        <time datetime="#_">5</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(6)">
                        <time datetime="#_">6</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(7)">
                        <time datetime="#_">7</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(8)">
                        <time datetime="#_">8</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(9)">
                        <time datetime="#_">9</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(10)">
                        <time datetime="#_">10</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(11)">
                        <time datetime="#_">11</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(12)">
                        <time datetime="#_">12</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(13)">
                        <time datetime="#_">13</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(14)">
                        <time datetime="#_">14</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(15)">
                        <time datetime="#_">15</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(16)">
                        <time datetime="#_">16</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(17)">
                        <time datetime="#_">17</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(18)">
                        <time datetime="#_">18</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(19)">
                        <time datetime="#_">19</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(20)">
                        <time datetime="#_">20</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(21)">
                        <time datetime="#_">21</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(22)">
                        <time datetime="#_">22</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(23)">
                        <time datetime="#_">23</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(24)">
                        <time datetime="#_">24</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(25)">
                        <time datetime="#_">25</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(26)">
                        <time datetime="#_">26</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(27)">
                        <time datetime="#_">27</time>
                    </button>
                    <button type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(28)">
                        <time datetime="#_">28</time>
                    </button>
                    <button id="29" type="button" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(29)">
                        <time datetime="#_">29</time>
                    </button>
                    <button id="30" type="button" style="display:none" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(30)">
                        <time datetime="#_">30</time>
                    </button>
                    <button id="31" type="button" style="display:none" class="focus:rounded-full focus:bg-[#EFB2CE] focus:text-black hover:rounded-full hover:border-2 mx-auto flex size-10 w-full items-center justify-center text-white" @click="changeSelectedDate(31)">
                        <time datetime="#_">31</time>
                    </button>
                </div>
            </div>
        </div>
        
        <hr class="w-full h-0.1 mx-auto mt-4 bg-white">
    </div>
</template>