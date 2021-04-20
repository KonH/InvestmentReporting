<template>
	<div :class="classes" style="width: 20rem">
		<div class="card-body">
			<h4>{{ dashboard.name }}</h4>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { DashboardConfigDto } from '@/api/meta';

@Options({
	name: 'DashboardCard',
})
export default class DashboardCard extends Vue {
	@Prop()
	dashboard!: DashboardConfigDto;

	@Prop()
	active!: boolean;

	get classes() {
		const baseClass = 'card';
		if (this.active) {
			return baseClass + ' bg-light';
		}
		return baseClass;
	}
}
</script>
