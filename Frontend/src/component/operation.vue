<template>
	<td>{{ operation.date }}</td>
	<td>{{ operation.amount }}</td>
	<td>{{ operation.kind }}</td>
	<td>{{ operation.category }}</td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { OperationDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'Operation',
})
export default class Operation extends Vue {
	@Prop()
	operation!: OperationDto;
}
</script>
