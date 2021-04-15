<template>
	<nav-bar-view />
	<div class="container container-lg">
		<router-view />
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Action } from 'vuex-class';
import NavBarView from '@/view/navBarView.vue';

@Options({
	name: 'App',
	components: {
		NavBarView,
	},
})
export default class App extends Vue {
	@Action('fetchState')
	fetchState!: () => void;

	@Action('fetchVirtualState')
	fetchVirtualState!: () => void;

	created() {
		this.fetchState();
		this.fetchVirtualState();
	}
}
</script>
